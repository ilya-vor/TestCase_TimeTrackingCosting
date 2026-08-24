using MongoDB.Bson;

namespace TimeTracking.Application.Common;

/// <summary>
/// $lookup-стадия, которая для каждой записи табеля находит ставку сотрудника,
/// действовавшую на дату записи. Резолв полностью на стороне MongoDB:
/// смена ставки задним числом автоматически меняет результат отчёта и итогов.
/// </summary>
public static class RateLookupStage
{
    public static BsonDocument Build() => new("$lookup", new BsonDocument
    {
        { "from", "employees" },
        { "let", new BsonDocument
            {
                { "eid", "$employeeId" },
                { "d", "$date" }
            }
        },
        { "pipeline", new BsonArray
            {
                new BsonDocument("$match", new BsonDocument("$expr",
                    new BsonDocument("$eq", new BsonArray { "$_id", "$$eid" }))),
                new BsonDocument("$unwind", "$rates"),
                new BsonDocument("$match", new BsonDocument("$expr",
                    new BsonDocument("$lte", new BsonArray { "$rates.from", "$$d" }))),
                new BsonDocument("$sort", new BsonDocument("rates.from", -1)),
                new BsonDocument("$limit", 1),
                new BsonDocument("$project", new BsonDocument
                    {
                        { "rate", "$rates.value" },
                        { "_id", 0 }
                    })
            }
        },
        { "as", "rate" }
    });

    /// <summary>Распаковка результата lookup: без совпадения (нет ставки) — null, остальные записи сохраняются.</summary>
    public static BsonDocument Unwind() => new("$unwind", new BsonDocument
    {
        { "path", "$rate" },
        { "preserveNullAndEmptyArrays", true }
    });
}
