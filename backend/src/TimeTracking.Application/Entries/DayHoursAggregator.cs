using MongoDB.Bson;
using MongoDB.Driver;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Entries;

/// <summary>
/// Агрегации часов сотрудников по дням (правила 2–3 и флаг переработки).
/// </summary>
internal static class DayHoursAggregator
{
    /// <summary>
    /// Сумма часов сотрудника за календарный день по всем проектам.
    /// </summary>
    public static async Task<double> SumForDayAsync(
        ITimeTrackingDb db, IClientSessionHandle? session, string employeeId, DateTime date, CancellationToken ct)
    {
        var dayStart = date.Date.ToUniversalTime();
        var dayEnd = dayStart.AddDays(1);

        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "employeeId", employeeId },
                { "date", new BsonDocument { { "$gte", dayStart }, { "$lt", dayEnd } } }
            }),
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

    /// <summary>
    /// Дневные тоталы сотрудников за месяц по полному дню (без фильтра по проекту),
    /// нужны для флага переработки в списке.
    /// </summary>
    public static async Task<Dictionary<(string EmployeeId, DateTime Date), double>> LoadMonthTotalsAsync(
        ITimeTrackingDb db, List<string> employeeIds, DateTime from, DateTime to, CancellationToken ct)
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

        var cursor = await db.TimeEntries.AggregateAsync<DayTotalDoc>(pipeline, cancellationToken: ct);
        var rows = await MongoCursorHelpers.ToListAsync(cursor, ct);

        var map = new Dictionary<(string, DateTime), double>();
        foreach (var row in rows)
            map[(row.EmployeeId, row.Date.Date)] = row.Hours;
        return map;
    }

    private class DayTotalDoc
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public double Hours { get; set; }
    }
}
