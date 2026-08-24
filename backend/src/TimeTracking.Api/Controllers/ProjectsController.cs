using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Projects;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<List<ProjectDto>> List()
        => _mediator.Send(new GetProjectsQuery());
}
