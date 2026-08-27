using MongoDB.Bson;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Reports;

/// <summary>
/// Пайплайн отчёта по проектам за месяц. Вся агрегация на стороне MongoDB:
/// $match по месяцу, резолв ставки на дату, стоимость, $group по проекту, проект, проценты и признаки.
/// В память выгружаются только строки по проектам.
/// </summary>
internal static class ProjectReportPipeline
{
    public static BsonDocument[] Build(int year, int month)
    {
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        return new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "date", new BsonDocument { { "$gte", from }, { "$lt", to } } }
            }),
            RateLookupStage.Build(),
            RateLookupStage.Unwind(),
            new BsonDocument("$project", new BsonDocument
            {
                { "projectId", 1 },
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
                { "_id", "$projectId" },
                { "hours", new BsonDocument("$sum", "$hours") },
                { "amount", new BsonDocument("$sum", "$cost") }
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "projects" },
                { "localField", "_id" },
                { "foreignField", "_id" },
                { "as", "project" }
            }),
            new BsonDocument("$unwind", "$project"),
            new BsonDocument("$project", new BsonDocument
            {
                { "projectId", "$_id" },
                { "code", "$project.code" },
                { "name", "$project.name" },
                { "budget", "$project.budget" },
                { "hours", new BsonDocument("$round", new BsonArray { "$hours", 1 }) },
                { "amount", new BsonDocument("$round", new BsonArray { "$amount", 2 }) },
                { "percent", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$gt", new BsonArray { "$project.budget", 0 }),
                        new BsonDocument("$round", new BsonArray
                        {
                            new BsonDocument("$multiply", new BsonArray
                            {
                                new BsonDocument("$divide", new BsonArray { "$amount", "$project.budget" }),
                                100
                            }),
                            2
                        }),
                        BsonNull.Value
                    })
                }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "projectId", 1 },
                { "code", 1 },
                { "name", 1 },
                { "budget", 1 },
                { "hours", 1 },
                { "amount", 1 },
                { "percent", 1 },
                { "overspent", new BsonDocument("$gt", new BsonArray
                    {
                        new BsonDocument("$ifNull", new BsonArray { "$percent", 0 }),
                        100
                    })
                },
                { "atRisk", new BsonDocument("$gt", new BsonArray
                    {
                        new BsonDocument("$ifNull", new BsonArray { "$percent", 0 }),
                        80
                    })
                }
            }),
            new BsonDocument("$sort", new BsonDocument("code", 1))
        };
    }
}
