using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Projects;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    public Task<List<ProjectDto>> List()
        => _mediator.Send(new GetProjectsQuery());
}
