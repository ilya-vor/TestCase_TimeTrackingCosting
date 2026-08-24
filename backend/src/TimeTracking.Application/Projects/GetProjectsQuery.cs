using MediatR;
using MongoDB.Driver;
using TimeTracking.Application.Common;
using TimeTracking.Domain;

namespace TimeTracking.Application.Projects;

public class ProjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
}

public class GetProjectsQuery : IRequest<List<ProjectDto>>
{
}

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly ITimeTrackingDb _db;

    public GetProjectsQueryHandler(ITimeTrackingDb db) => _db = db;

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var projects = await _db.Projects.Find(Builders<Project>.Filter.Empty)
            .SortBy(p => p.Code)
            .ToListAsync(ct);

        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Start = p.Start,
            End = p.End
        }).ToList();
    }
}
