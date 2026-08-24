using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Entries;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/time-entries")]
public class TimeEntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TimeEntriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public Task<TimeEntryPageResult> List(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string? employeeId,
        [FromQuery] string? projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => _mediator.Send(new GetTimeEntriesQuery
        {
            Year = year,
            Month = month,
            EmployeeId = employeeId,
            ProjectId = projectId,
            Page = page,
            PageSize = pageSize
        });

    [HttpPut]
    public Task<TimeEntryRow> Create([FromBody] CreateTimeEntryCommand command)
        => _mediator.Send(command);

    [HttpPost("{id}")]
    public Task<TimeEntryRow> Update(string id, [FromBody] UpdateTimeEntryCommand command)
    {
        command.Id = id;
        return _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mediator.Send(new DeleteTimeEntryCommand { Id = id });
        return NoContent();
    }
}
