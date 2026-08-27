using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Reports;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("projects")]
    public Task<List<ProjectReportRow>> Projects([FromQuery] int year, [FromQuery] int month)
        => _mediator.Send(new GetProjectReportQuery { Year = year, Month = month });
}
