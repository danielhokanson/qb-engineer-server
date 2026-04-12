using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Mrp;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/mrp")]
[Authorize(Roles = "Admin,Manager")]
public class MrpController(IMediator mediator) : ControllerBase
{
    // === MRP Runs ===

    [HttpGet("runs")]
    public async Task<ActionResult<List<MrpRunResponseModel>>> GetRuns()
    {
        var result = await mediator.Send(new GetMrpRunsQuery());
        return Ok(result);
    }

    [HttpGet("runs/{id:int}")]
    public async Task<ActionResult<MrpRunResponseModel>> GetRun(int id)
    {
        var result = await mediator.Send(new GetMrpRunDetailQuery(id));
        return Ok(result);
    }

    [HttpPost("runs")]
    public async Task<ActionResult<MrpRunResponseModel>> ExecuteRun([FromBody] ExecuteMrpRunRequest request)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
        var result = await mediator.Send(new ExecuteMrpRunCommand(
            request.RunType,
            request.PlanningHorizonDays,
            request.PartIds,
            userId
        ));
        return CreatedAtAction(nameof(GetRun), new { id = result.Id }, result);
    }

    [HttpPost("runs/simulate")]
    public async Task<ActionResult<MrpRunResponseModel>> SimulateRun([FromBody] ExecuteMrpRunRequest request)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
        var result = await mediator.Send(new SimulateMrpRunCommand(
            MrpRunType.Simulation,
            request.PlanningHorizonDays,
            request.PartIds,
            userId
        ));
        return Ok(result);
    }

    // === Planned Orders ===

    [HttpGet("planned-orders")]
    public async Task<ActionResult<List<MrpPlannedOrderResponseModel>>> GetPlannedOrders(
        [FromQuery] int? mrpRunId,
        [FromQuery] MrpPlannedOrderStatus? status)
    {
        var result = await mediator.Send(new GetPlannedOrdersQuery(mrpRunId, status));
        return Ok(result);
    }

    [HttpPatch("planned-orders/{id:int}")]
    public async Task<IActionResult> UpdatePlannedOrder(int id, [FromBody] UpdatePlannedOrderRequest request)
    {
        await mediator.Send(new UpdatePlannedOrderCommand(id, request.IsFirmed, request.Notes));
        return NoContent();
    }

    [HttpPost("planned-orders/{id:int}/release")]
    public async Task<ActionResult<ReleasePlannedOrderResult>> ReleasePlannedOrder(int id)
    {
        var result = await mediator.Send(new ReleasePlannedOrderCommand(id));
        return Ok(result);
    }

    [HttpPost("planned-orders/bulk-release")]
    public async Task<ActionResult<List<ReleasePlannedOrderResult>>> BulkReleasePlannedOrders([FromBody] BulkReleaseRequest request)
    {
        var result = await mediator.Send(new BulkReleasePlannedOrdersCommand(request.Ids));
        return Ok(result);
    }

    [HttpDelete("planned-orders/{id:int}")]
    public async Task<IActionResult> DeletePlannedOrder(int id)
    {
        await mediator.Send(new DeletePlannedOrderCommand(id));
        return NoContent();
    }

    // === Exceptions ===

    [HttpGet("exceptions")]
    public async Task<ActionResult<List<MrpExceptionResponseModel>>> GetExceptions(
        [FromQuery] int? mrpRunId,
        [FromQuery] bool? unresolvedOnly)
    {
        var result = await mediator.Send(new GetMrpExceptionsQuery(mrpRunId, unresolvedOnly));
        return Ok(result);
    }

    [HttpPost("exceptions/{id:int}/resolve")]
    public async Task<IActionResult> ResolveException(int id, [FromBody] ResolveExceptionRequest request)
    {
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
        await mediator.Send(new ResolveMrpExceptionCommand(id, request.ResolutionNotes, userId));
        return NoContent();
    }

    // === Part Plan & Pegging ===

    [HttpGet("runs/{runId:int}/parts/{partId:int}/plan")]
    public async Task<ActionResult<MrpPartPlanResponseModel>> GetPartPlan(int runId, int partId)
    {
        var result = await mediator.Send(new GetMrpPartPlanQuery(runId, partId));
        return Ok(result);
    }

    [HttpGet("runs/{runId:int}/parts/{partId:int}/pegging")]
    public async Task<ActionResult<List<MrpPeggingResponseModel>>> GetPegging(int runId, int partId)
    {
        var result = await mediator.Send(new GetMrpPeggingQuery(runId, partId));
        return Ok(result);
    }
}

// Request models
public record ExecuteMrpRunRequest(
    MrpRunType RunType = MrpRunType.Full,
    int PlanningHorizonDays = 90,
    List<int>? PartIds = null
);

public record UpdatePlannedOrderRequest(bool? IsFirmed, string? Notes);
public record BulkReleaseRequest(List<int> Ids);
public record ResolveExceptionRequest(string? ResolutionNotes);
