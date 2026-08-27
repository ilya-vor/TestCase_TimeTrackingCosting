using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

public class GetTimeEntriesQueryHandler(ITimeTrackingDb _db) : IRequestHandler<GetTimeEntriesQuery, TimeEntryPageResult>
{
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

        // В память попадает только страница. Сортировка по _id — стабильный tiebreaker
        // для skip/limit-пагинации (иначе записи с одной датой могут «прыгать» между страницами).
        var entries = await _db.TimeEntries.Find(filter)
            .Sort(Builders<TimeEntry>.Sort.Ascending(e => e.Date).Ascending(e => e.Id))
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

        // Дневные тоталы по полному дню сотрудника (без фильтра по проекту),
        // иначе флаг переработки был бы неверным при фильтре по проекту.
        var dayTotals = await DayHoursAggregator.LoadMonthTotalsAsync(_db, employeeIds, from, to, ct);

        var items = entries.Select(e => TimeEntryRowProjector.Build(
            e,
            employeeMap.GetValueOrDefault(e.EmployeeId),
            projectMap.GetValueOrDefault(e.ProjectId),
            dayTotals.GetValueOrDefault((e.EmployeeId, e.Date.Date)))).ToList();

        var totals = await EntryTotalsAggregator.LoadAsync(_db, from, to, query.EmployeeId, query.ProjectId, ct);

        return new TimeEntryPageResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalHours = totals.hours,
            TotalAmount = totals.amount
        };
    }
}
