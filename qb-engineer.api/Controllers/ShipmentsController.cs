using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Shipments;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/shipments")]
[Authorize]
public class ShipmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ShipmentListItemModel>>> GetShipments(
        [FromQuery] int? salesOrderId,
        [FromQuery] ShipmentStatus? status)
    {
        var result = await mediator.Send(new GetShipmentsQuery(salesOrderId, status));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShipmentDetailResponseModel>> GetShipment(int id)
    {
        var result = await mediator.Send(new GetShipmentByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentListItemModel>> CreateShipment(CreateShipmentRequestModel request)
    {
        var result = await mediator.Send(new CreateShipmentCommand(
            request.SalesOrderId, request.ShippingAddressId, request.Carrier,
            request.TrackingNumber, request.ShippingCost, request.Weight,
            request.Notes, request.Lines));
        return CreatedAtAction(nameof(GetShipment), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateShipment(int id, UpdateShipmentRequestModel request)
    {
        await mediator.Send(new UpdateShipmentCommand(
            id, request.Carrier, request.TrackingNumber,
            request.ShippingCost, request.Weight, request.Notes));
        return NoContent();
    }

    [HttpPost("{id:int}/ship")]
    public async Task<IActionResult> ShipShipment(int id)
    {
        await mediator.Send(new ShipShipmentCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/packing-slip")]
    public async Task<IActionResult> GetPackingSlip(int id)
    {
        var pdf = await mediator.Send(new GeneratePackingSlipPdfQuery(id));
        return File(pdf, "application/pdf", $"packing-slip-{id}.pdf");
    }

    [HttpPost("{id:int}/deliver")]
    public async Task<IActionResult> DeliverShipment(int id)
    {
        await mediator.Send(new DeliverShipmentCommand(id));
        return NoContent();
    }
}
