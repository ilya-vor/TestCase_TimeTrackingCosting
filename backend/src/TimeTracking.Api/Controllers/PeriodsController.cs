using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Periods;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/periods")]
public class PeriodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PeriodsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("close")]
    public async Task<IActionResult> Close([FromBody] ClosePeriodCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] OpenPeriodCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
