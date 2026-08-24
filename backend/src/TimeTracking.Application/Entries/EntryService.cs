using MongoDB.Bson;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Application.Rules;
using TimeTracking.Domain;

namespace TimeTracking.Application.Entries;

internal static class EntryService
{
    /// <summary>Сумма часов сотрудника за календарный день по всем проектам.</summary>
    public static async Task<double> SumDayHoursAsync(
        ITimeTrackingDb db, IClientSessionHandle? session, string employeeId, DateTime date, CancellationToken ct)
    {
        var dayStart = date.Date.ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var match = new BsonDocument("$match", new BsonDocument
        {
            { "employeeId", employeeId },
            { "date", new BsonDocument { { "$gte", dayStart }, { "$lt", dayEnd } } }
        });

        var pipeline = new BsonDocument[]
        {
            match,
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "total", new BsonDocument("$sum", "$hours") }
            })
        };

        var cursor = session is null
            ? await db.TimeEntries.AggregateAsync<BsonDocument>(pipeline, cancellationToken: ct)
            : await db.TimeEntries.AggregateAsync<BsonDocument>(session, pipeline, cancellationToken: ct);

        var doc = await MongoCursorHelpers.FirstOrDefaultAsync(cursor, ct);
        return doc is null ? 0 : doc["total"].ToDouble();
    }

    public static async Task<TimeEntryRow> BuildRowAsync(
        ITimeTrackingDb db, IClientSessionHandle? session, TimeEntry entry, CancellationToken ct)
    {
        var employee = session is null
            ? await db.Employees.Find(e => e.Id == entry.EmployeeId).FirstOrDefaultAsync(ct)
            : await db.Employees.Find(session, e => e.Id == entry.EmployeeId).FirstOrDefaultAsync(ct);
        var project = session is null
            ? await db.Projects.Find(p => p.Id == entry.ProjectId).FirstOrDefaultAsync(ct)
            : await db.Projects.Find(session, p => p.Id == entry.ProjectId).FirstOrDefaultAsync(ct);
        var rate = employee is null ? 0m : EmployeeRates.EffectiveOn(employee.Rates, entry.Date)?.Value ?? 0m;
        var amount = Money.Round((decimal)entry.Hours * rate);
        var dayTotal = await SumDayHoursAsync(db, session, entry.EmployeeId, entry.Date, ct);

        return new TimeEntryRow
        {
            Id = entry.Id,
            EmployeeId = entry.EmployeeId,
            EmployeeName = employee?.Name ?? "—",
            ProjectId = entry.ProjectId,
            ProjectCode = project?.Code ?? "—",
            Date = entry.Date,
            Hours = entry.Hours,
            Rate = rate,
            Amount = amount,
            Comment = entry.Comment,
            Overtime = DayHoursLimitRule.IsOvertime(dayTotal),
            Version = entry.Version
        };
    }
}
