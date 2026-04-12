using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Scheduling;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/shifts")]
[Authorize(Roles = "Admin,Manager")]
public class ShiftsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ShiftResponseModel>>> GetAll()
    {
        var result = await mediator.Send(new GetShiftsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponseModel>> Create([FromBody] CreateShiftRequest request)
    {
        var result = await mediator.Send(new CreateShiftCommand(
            request.Name, request.StartTime, request.EndTime, request.BreakMinutes));
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShiftResponseModel>> Update(int id, [FromBody] UpdateShiftRequest request)
    {
        var result = await mediator.Send(new UpdateShiftCommand(
            id, request.Name, request.StartTime, request.EndTime,
            request.BreakMinutes, request.IsActive));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteShiftCommand(id));
        return NoContent();
    }
}

public record CreateShiftRequest(string Name, TimeOnly StartTime, TimeOnly EndTime, int BreakMinutes);

public record UpdateShiftRequest(string Name, TimeOnly StartTime, TimeOnly EndTime, int BreakMinutes, bool IsActive);
