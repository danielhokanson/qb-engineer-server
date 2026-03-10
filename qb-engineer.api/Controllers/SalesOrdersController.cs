using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.SalesOrders;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class SalesOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SalesOrderListItemModel>>> GetSalesOrders(
        [FromQuery] int? customerId,
        [FromQuery] SalesOrderStatus? status)
    {
        var result = await mediator.Send(new GetSalesOrdersQuery(customerId, status));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SalesOrderDetailResponseModel>> GetSalesOrder(int id)
    {
        var result = await mediator.Send(new GetSalesOrderByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<SalesOrderListItemModel>> CreateSalesOrder(CreateSalesOrderRequestModel request)
    {
        var result = await mediator.Send(new CreateSalesOrderCommand(
            request.CustomerId, request.QuoteId, request.ShippingAddressId,
            request.BillingAddressId, request.CreditTerms, request.RequestedDeliveryDate,
            request.CustomerPO, request.Notes, request.TaxRate, request.Lines));
        return CreatedAtAction(nameof(GetSalesOrder), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSalesOrder(int id, UpdateSalesOrderRequestModel request)
    {
        await mediator.Send(new UpdateSalesOrderCommand(
            id, request.ShippingAddressId, request.BillingAddressId,
            request.CreditTerms, request.RequestedDeliveryDate,
            request.CustomerPO, request.Notes, request.TaxRate));
        return NoContent();
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmSalesOrder(int id)
    {
        await mediator.Send(new ConfirmSalesOrderCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelSalesOrder(int id)
    {
        await mediator.Send(new CancelSalesOrderCommand(id));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSalesOrder(int id)
    {
        await mediator.Send(new DeleteSalesOrderCommand(id));
        return NoContent();
    }
}
