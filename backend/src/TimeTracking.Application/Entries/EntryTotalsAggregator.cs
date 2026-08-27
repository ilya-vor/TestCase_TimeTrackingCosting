using MongoDB.Bson;
using MongoDB.Driver;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Entries;

/// <summary>
/// Итоги (часы и стоимость) по всей отфильтрованной выборке списка.
/// Стоимость считается по ставке на дату записи, как в отчёте.
/// </summary>
internal static class EntryTotalsAggregator
{
    public static async Task<(double hours, decimal amount)> LoadAsync(
        ITimeTrackingDb db, DateTime from, DateTime to, string? employeeId, string? projectId, CancellationToken ct)
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

        var cursor = await db.TimeEntries.AggregateAsync<BsonDocument>(pipeline, cancellationToken: ct);
        var doc = await MongoCursorHelpers.FirstOrDefaultAsync(cursor, ct);

        if (doc is null)
            return (0, 0m);

        return (doc["hours"].ToDouble(), Money.Round(doc["amount"].ToDecimal()));
    }
}
