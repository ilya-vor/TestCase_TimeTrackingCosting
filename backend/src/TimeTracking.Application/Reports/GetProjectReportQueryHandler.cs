using MediatR;
using MongoDB.Bson;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Reports;

public class GetProjectReportQueryHandler : IRequestHandler<GetProjectReportQuery, List<ProjectReportRow>>
{
    private readonly ITimeTrackingDb _db;

    public GetProjectReportQueryHandler(ITimeTrackingDb db) => _db = db;

    public async Task<List<ProjectReportRow>> Handle(GetProjectReportQuery query, CancellationToken ct)
    {
        var from = new DateTime(query.Year, query.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        // Отчёт целиком считается агрегацией на стороне MongoDB:
        // в память выгружаются только строки по проектам, а не сырые записи.
        var pipeline = new BsonDocument[]
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

        var rows = await _db.TimeEntries.AggregateAsync<ProjectReportRow>(pipeline, cancellationToken: ct);
        var list = await MongoCursorHelpers.ToListAsync(rows, ct);

        // Итоговая строка — сумма по уже агрегированным проектам (не по сырым записям).
        list.Add(new ProjectReportRow
        {
            Code = "ИТОГО",
            Name = "Итого",
            Hours = Math.Round(list.Sum(r => r.Hours), 1),
            Amount = Money.Round(list.Sum(r => r.Amount)),
            Budget = 0,
            Percent = null
        });

        return list;
    }
}
