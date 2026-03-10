using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.TimeTracking;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/time-tracking")]
[Authorize]
public class TimeTrackingController(IMediator mediator) : ControllerBase
{
    [HttpGet("entries")]
    public async Task<ActionResult<List<TimeEntryResponseModel>>> GetTimeEntries(
        [FromQuery] int? userId,
        [FromQuery] int? jobId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var result = await mediator.Send(new GetTimeEntriesQuery(userId, jobId, from, to));
        return Ok(result);
    }

    [HttpPost("entries")]
    public async Task<ActionResult<TimeEntryResponseModel>> CreateTimeEntry([FromBody] CreateTimeEntryRequestModel request)
    {
        var result = await mediator.Send(new CreateTimeEntryCommand(request));
        return Created($"/api/v1/time-tracking/entries/{result.Id}", result);
    }

    [HttpPatch("entries/{id:int}")]
    public async Task<ActionResult<TimeEntryResponseModel>> UpdateTimeEntry(int id, [FromBody] UpdateTimeEntryRequestModel request)
    {
        var result = await mediator.Send(new UpdateTimeEntryCommand(id, request));
        return Ok(result);
    }

    [HttpPost("timer/start")]
    public async Task<ActionResult<TimeEntryResponseModel>> StartTimer([FromBody] StartTimerRequestModel request)
    {
        var result = await mediator.Send(new StartTimerCommand(request));
        return Created($"/api/v1/time-tracking/entries/{result.Id}", result);
    }

    [HttpPost("timer/stop")]
    public async Task<ActionResult<TimeEntryResponseModel>> StopTimer([FromBody] StopTimerRequestModel request)
    {
        var result = await mediator.Send(new StopTimerCommand(request));
        return Ok(result);
    }

    [HttpGet("clock-events")]
    public async Task<ActionResult<List<ClockEventResponseModel>>> GetClockEvents(
        [FromQuery] int? userId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var result = await mediator.Send(new GetClockEventsQuery(userId, from, to));
        return Ok(result);
    }

    [HttpPost("clock-events")]
    public async Task<ActionResult<ClockEventResponseModel>> CreateClockEvent([FromBody] CreateClockEventRequestModel request)
    {
        var result = await mediator.Send(new CreateClockEventCommand(request));
        return Created($"/api/v1/time-tracking/clock-events/{result.Id}", result);
    }

    [HttpDelete("entries/{id:int}")]
    public async Task<IActionResult> DeleteTimeEntry(int id)
    {
        await mediator.Send(new DeleteTimeEntryCommand(id));
        return NoContent();
    }

    // ─── Pay Period ───

    [HttpGet("pay-period")]
    public async Task<ActionResult<PayPeriodResponseModel>> GetCurrentPayPeriod()
    {
        var result = await mediator.Send(new GetCurrentPayPeriodQuery());
        return Ok(result);
    }

    [HttpPut("pay-period/settings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePayPeriodSettings([FromBody] UpdatePayPeriodSettingsCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}
