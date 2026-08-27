using MediatR;
using TimeTracking.Application.Common;

namespace TimeTracking.Application.Reports;

public class GetProjectReportQueryHandler(ITimeTrackingDb _db) : IRequestHandler<GetProjectReportQuery, List<ProjectReportRow>>
{
    public async Task<List<ProjectReportRow>> Handle(GetProjectReportQuery query, CancellationToken ct)
    {
        var pipeline = ProjectReportPipeline.Build(query.Year, query.Month);

        var list = await MongoCursorHelpers.ToListAsync(
            await _db.TimeEntries.AggregateAsync<ProjectReportRow>(pipeline, cancellationToken: ct), ct);

        list.Add(BuildTotalRow(list));
        return list;
    }

    private static ProjectReportRow BuildTotalRow(List<ProjectReportRow> rows) => new()
    {
        Code = "ИТОГО",
        Name = "Итого",
        Hours = Math.Round(rows.Sum(r => r.Hours), 1),
        Amount = Money.Round(rows.Sum(r => r.Amount)),
        Budget = 0,
        Percent = null
    };
}
