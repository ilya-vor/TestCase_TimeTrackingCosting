using MediatR;
using Microsoft.AspNetCore.Mvc;
using TimeTracking.Application.Employees;

namespace TimeTracking.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    public Task<List<EmployeeDto>> List()
        => _mediator.Send(new GetEmployeesQuery());

    /// <summary>
    /// Задание не описывает такой endpoint, но приёмочный сценарий 8 требует смены ставки
    /// задним числом. Upsert ставки по дате начала действия.
    /// </summary>
    [HttpPut("{id}/rates")]
    public async Task<IActionResult> SetRate(string id, [FromBody] SetEmployeeRateCommand command)
    {
        command.EmployeeId = id;
        await _mediator.Send(command);
        return NoContent();
    }
}
