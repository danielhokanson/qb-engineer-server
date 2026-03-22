using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.PurchaseOrders;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/purchase-orders")]
[Authorize]
public class PurchaseOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PurchaseOrderListItemModel>>> GetPurchaseOrders(
        [FromQuery] int? vendorId,
        [FromQuery] int? jobId,
        [FromQuery] PurchaseOrderStatus? status)
    {
        var result = await mediator.Send(new GetPurchaseOrdersQuery(vendorId, jobId, status));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseOrderDetailResponseModel>> GetPurchaseOrder(int id)
    {
        var result = await mediator.Send(new GetPurchaseOrderByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrderListItemModel>> CreatePurchaseOrder(CreatePurchaseOrderRequestModel request)
    {
        var result = await mediator.Send(new CreatePurchaseOrderCommand(
            request.VendorId, request.JobId, request.Notes, request.Lines));
        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePurchaseOrder(int id, UpdatePurchaseOrderRequestModel request)
    {
        await mediator.Send(new UpdatePurchaseOrderCommand(id, request.Notes, request.ExpectedDeliveryDate));
        return NoContent();
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> SubmitPurchaseOrder(int id)
    {
        await mediator.Send(new SubmitPurchaseOrderCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/acknowledge")]
    public async Task<IActionResult> AcknowledgePurchaseOrder(int id, AcknowledgePurchaseOrderRequestModel request)
    {
        await mediator.Send(new AcknowledgePurchaseOrderCommand(id, request.ExpectedDeliveryDate));
        return NoContent();
    }

    [HttpPost("{id:int}/receive")]
    public async Task<IActionResult> ReceiveItems(int id, ReceiveItemsRequestModel request)
    {
        await mediator.Send(new ReceiveItemsCommand(id, request.Lines));
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelPurchaseOrder(int id)
    {
        await mediator.Send(new CancelPurchaseOrderCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> ClosePurchaseOrder(int id)
    {
        await mediator.Send(new ClosePurchaseOrderCommand(id));
        return NoContent();
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<List<PoCalendarResponseModel>>> GetForCalendar(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var result = await mediator.Send(new GetPurchaseOrdersForCalendarQuery(from, to));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePurchaseOrder(int id)
    {
        await mediator.Send(new DeletePurchaseOrderCommand(id));
        return NoContent();
    }
}
