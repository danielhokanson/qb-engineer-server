using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Inventory;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("locations")]
    public async Task<ActionResult<List<StorageLocationResponseModel>>> GetLocationTree()
    {
        var result = await mediator.Send(new GetLocationTreeQuery());
        return Ok(result);
    }

    [HttpGet("locations/bins")]
    public async Task<ActionResult<List<StorageLocationFlatResponseModel>>> GetBinLocations()
    {
        var result = await mediator.Send(new GetBinLocationsQuery());
        return Ok(result);
    }

    [HttpPost("locations")]
    public async Task<ActionResult<StorageLocationResponseModel>> CreateLocation([FromBody] CreateStorageLocationRequestModel request)
    {
        var result = await mediator.Send(new CreateStorageLocationCommand(request));
        return Created($"/api/v1/inventory/locations", result);
    }

    [HttpGet("locations/{locationId:int}/contents")]
    public async Task<ActionResult<List<BinContentResponseModel>>> GetBinContents(int locationId)
    {
        var result = await mediator.Send(new GetBinContentsQuery(locationId));
        return Ok(result);
    }

    [HttpPost("bin-contents")]
    public async Task<ActionResult<BinContentResponseModel>> PlaceBinContent([FromBody] PlaceBinContentRequestModel request)
    {
        var result = await mediator.Send(new PlaceBinContentCommand(request));
        return Created($"/api/v1/inventory/bin-contents/{result.Id}", result);
    }

    [HttpGet("parts")]
    public async Task<ActionResult<List<InventoryPartSummaryResponseModel>>> GetPartInventory([FromQuery] string? search)
    {
        var result = await mediator.Send(new GetPartInventoryQuery(search));
        return Ok(result);
    }

    [HttpGet("movements")]
    public async Task<ActionResult<List<BinMovementResponseModel>>> GetMovements(
        [FromQuery] int? locationId,
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] int take = 100)
    {
        var result = await mediator.Send(new GetMovementsQuery(locationId, entityType, entityId, take));
        return Ok(result);
    }

    [HttpDelete("locations/{id:int}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        await mediator.Send(new DeleteStorageLocationCommand(id));
        return NoContent();
    }

    [HttpDelete("bin-contents/{id:int}")]
    public async Task<IActionResult> RemoveBinContent(int id)
    {
        await mediator.Send(new RemoveBinContentCommand(id));
        return NoContent();
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockAlertModel>>> GetLowStockAlerts()
    {
        var result = await mediator.Send(new GetLowStockAlertsQuery());
        return Ok(result);
    }

    // ── Receiving ──

    [HttpPost("receive")]
    public async Task<ActionResult<ReceivingRecordResponseModel>> ReceiveGoods(
        [FromBody] ReceivePurchaseOrderRequestModel request)
    {
        var result = await mediator.Send(new ReceivePurchaseOrderCommand(request));
        return Created($"/api/v1/inventory/receiving-history", result);
    }

    [HttpGet("receiving-history")]
    public async Task<ActionResult<List<ReceivingRecordResponseModel>>> GetReceivingHistory(
        [FromQuery] int? purchaseOrderId,
        [FromQuery] int? partId,
        [FromQuery] int take = 50)
    {
        var result = await mediator.Send(new GetReceivingHistoryQuery(purchaseOrderId, partId, take));
        return Ok(result);
    }

    // ── Stock Operations ──

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] TransferStockRequestModel request)
    {
        await mediator.Send(new TransferStockCommand(request));
        return NoContent();
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequestModel request)
    {
        await mediator.Send(new AdjustStockCommand(request));
        return NoContent();
    }

    // ── Cycle Counts ──

    [HttpGet("cycle-counts")]
    public async Task<ActionResult<List<CycleCountResponseModel>>> GetCycleCounts(
        [FromQuery] int? locationId,
        [FromQuery] string? status)
    {
        var result = await mediator.Send(new GetCycleCountsQuery(locationId, status));
        return Ok(result);
    }

    [HttpPost("cycle-counts")]
    public async Task<ActionResult<CycleCountResponseModel>> CreateCycleCount(
        [FromBody] CreateCycleCountRequestModel request)
    {
        var result = await mediator.Send(new CreateCycleCountCommand(request));
        return Created($"/api/v1/inventory/cycle-counts/{result.Id}", result);
    }

    [HttpPut("cycle-counts/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateCycleCount(int id, [FromBody] UpdateCycleCountRequestModel request)
    {
        await mediator.Send(new UpdateCycleCountCommand(id, request));
        return NoContent();
    }

    // ── Reservations ──

    [HttpGet("reservations")]
    public async Task<ActionResult<List<ReservationResponseModel>>> GetReservations(
        [FromQuery] int? partId,
        [FromQuery] int? jobId)
    {
        var result = await mediator.Send(new GetReservationsQuery(partId, jobId));
        return Ok(result);
    }

    [HttpPost("reservations")]
    public async Task<ActionResult<ReservationResponseModel>> CreateReservation(
        [FromBody] CreateReservationRequestModel request)
    {
        var result = await mediator.Send(new CreateReservationCommand(request));
        return Created($"/api/v1/inventory/reservations/{result.Id}", result);
    }

    [HttpDelete("reservations/{id:int}")]
    public async Task<IActionResult> ReleaseReservation(int id)
    {
        await mediator.Send(new ReleaseReservationCommand(id));
        return NoContent();
    }
}
