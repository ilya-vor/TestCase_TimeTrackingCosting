using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

public class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, TimeEntryPageResult>
{
    private readonly ITimeTrackingDb _db;

    public GetTimeEntriesQueryHandler(ITimeTrackingDb db) => _db = db;

    public async Task<TimeEntryPageResult> Handle(GetTimeEntriesQuery query, CancellationToken ct)
    {
        var from = new DateTime(query.Year, query.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        var filter = Builders<TimeEntry>.Filter.And(
            Builders<TimeEntry>.Filter.Gte(e => e.Date, from),
            Builders<TimeEntry>.Filter.Lt(e => e.Date, to));
        if (!string.IsNullOrWhiteSpace(query.EmployeeId))
            filter &= Builders<TimeEntry>.Filter.Eq(e => e.EmployeeId, query.EmployeeId);
        if (!string.IsNullOrWhiteSpace(query.ProjectId))
            filter &= Builders<TimeEntry>.Filter.Eq(e => e.ProjectId, query.ProjectId);

        var totalCount = await _db.TimeEntries.CountDocumentsAsync(filter, cancellationToken: ct);

        // Реальная пагинация на стороне БД: в память попадает только страница.
        var entries = await _db.TimeEntries.Find(filter)
            .Sort(Builders<TimeEntry>.Sort.Ascending(e => e.Date))
            .Skip((query.Page - 1) * query.PageSize)
            .Limit(query.PageSize)
            .ToListAsync(ct);

        var employeeIds = entries.Select(e => e.EmployeeId).Distinct().ToList();
        var projectIds = entries.Select(e => e.ProjectId).Distinct().ToList();

        var employees = employeeIds.Count == 0
            ? new List<Employee>()
            : await _db.Employees.Find(Builders<Employee>.Filter.In(e => e.Id, employeeIds)).ToListAsync(ct);
        var projects = projectIds.Count == 0
            ? new List<Project>()
            : await _db.Projects.Find(Builders<Project>.Filter.In(p => p.Id, projectIds)).ToListAsync(ct);

        var employeeMap = employees.ToDictionary(e => e.Id);
        var projectMap = projects.ToDictionary(p => p.Id);

        // Дневные тоталы по странице считаются по полному дню сотрудника
        // (без фильтра по проекту и без пагинации), иначе флаг переработки был бы неверным.
        var dayTotals = await LoadDayTotalsAsync(employeeIds, from, to, ct);

        var items = entries.Select(e =>
        {
            var employee = employeeMap.GetValueOrDefault(e.EmployeeId);
            var project = projectMap.GetValueOrDefault(e.ProjectId);
            var rate = employee is null ? 0m : EmployeeRates.EffectiveOn(employee.Rates, e.Date)?.Value ?? 0m;
            var amount = Money.Round((decimal)e.Hours * rate);
            var dayTotal = dayTotals.GetValueOrDefault((e.EmployeeId, e.Date.Date));

            return new TimeEntryRow
            {
                Id = e.Id,
                EmployeeId = e.EmployeeId,
                EmployeeName = employee?.Name ?? "—",
                ProjectId = e.ProjectId,
                ProjectCode = project?.Code ?? "—",
                Date = e.Date,
                Hours = e.Hours,
                Rate = rate,
                Amount = amount,
                Comment = e.Comment,
                Overtime = DayHoursLimitRule.IsOvertime(dayTotal),
                Version = e.Version
            };
        }).ToList();

        var totals = await LoadTotalsAsync(from, to, query.EmployeeId, query.ProjectId, ct);

        return new TimeEntryPageResult
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalHours = totals.hours,
            TotalAmount = totals.amount
        };
    }

    private async Task<Dictionary<(string EmployeeId, DateTime Date), double>> LoadDayTotalsAsync(
        List<string> employeeIds, DateTime from, DateTime to, CancellationToken ct)
    {
        if (employeeIds.Count == 0)
            return new Dictionary<(string, DateTime), double>();

        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "employeeId", new BsonDocument("$in", new BsonArray(employeeIds)) },
                { "date", new BsonDocument { { "$gte", from }, { "$lt", to } } }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument { { "employeeId", "$employeeId" }, { "date", "$date" } } },
                { "hours", new BsonDocument("$sum", "$hours") }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "employeeId", "$_id.employeeId" },
                { "date", "$_id.date" },
                { "hours", 1 }
            })
        };

        var cursor = await _db.TimeEntries.AggregateAsync<DayTotalDoc>(pipeline, cancellationToken: ct);
        var rows = await MongoCursorHelpers.ToListAsync(cursor, ct);

        var map = new Dictionary<(string, DateTime), double>();
        foreach (var row in rows)
            map[(row.EmployeeId, row.Date.Date)] = row.Hours;
        return map;
    }

    /// <summary>
    /// Итоги по всей отфильтрованной выборке (все страницы) — агрегацией на стороне MongoDB.
    /// Стоимость пересчитывается по ставке на дату записи (как в отчёте).
    /// </summary>
    private async Task<(double hours, decimal amount)> LoadTotalsAsync(
        DateTime from, DateTime to, string? employeeId, string? projectId, CancellationToken ct)
    {
        var dateFilter = new BsonDocument { { "$gte", from }, { "$lt", to } };
        var conditions = new BsonArray { new BsonDocument("date", dateFilter) };
        if (!string.IsNullOrWhiteSpace(employeeId))
            conditions.Add(new BsonDocument("employeeId", employeeId));
        if (!string.IsNullOrWhiteSpace(projectId))
            conditions.Add(new BsonDocument("projectId", projectId));

        var match = conditions.Count == 1
            ? new BsonDocument("$match", conditions[0].AsBsonDocument)
            : new BsonDocument("$match", new BsonDocument("$and", conditions));

        var pipeline = new BsonDocument[]
        {
            match,
            RateLookupStage.Build(),
            RateLookupStage.Unwind(),
            new BsonDocument("$project", new BsonDocument
            {
                { "hours", 1 },
                { "cost", new BsonDocument("$multiply", new BsonArray
                    {
                        "$hours",
                        new BsonDocument("$ifNull", new BsonArray { "$rate.rate", 0 })
                    })
                }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "hours", new BsonDocument("$sum", "$hours") },
                { "amount", new BsonDocument("$sum", "$cost") }
            })
        };

        var cursor = await _db.TimeEntries.AggregateAsync<BsonDocument>(pipeline, cancellationToken: ct);
        var doc = await MongoCursorHelpers.FirstOrDefaultAsync(cursor, ct);

        if (doc is null)
            return (0, 0m);

        return (doc["hours"].ToDouble(), Money.Round(doc["amount"].ToDecimal()));
    }

    private class DayTotalDoc
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Hours { get; set; }
    }
}
