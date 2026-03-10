using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Payments;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// ⚡ ACCOUNTING BOUNDARY — Standalone mode: full CRUD. Integrated mode: read-only.
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PaymentListItemModel>>> GetPayments(
        [FromQuery] int? customerId)
    {
        var result = await mediator.Send(new GetPaymentsQuery(customerId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDetailResponseModel>> GetPayment(int id)
    {
        var result = await mediator.Send(new GetPaymentByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentListItemModel>> CreatePayment(CreatePaymentRequestModel request)
    {
        var result = await mediator.Send(new CreatePaymentCommand(
            request.CustomerId, request.Method, request.Amount,
            request.PaymentDate, request.ReferenceNumber, request.Notes,
            request.Applications));
        return CreatedAtAction(nameof(GetPayment), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePayment(int id)
    {
        await mediator.Send(new DeletePaymentCommand(id));
        return NoContent();
    }
}
