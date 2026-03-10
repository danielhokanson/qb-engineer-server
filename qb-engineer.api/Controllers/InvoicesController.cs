using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Invoices;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// ⚡ ACCOUNTING BOUNDARY — Standalone mode: full CRUD. Integrated mode: read-only.
/// </summary>
[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public class InvoicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<InvoiceListItemModel>>> GetInvoices(
        [FromQuery] int? customerId,
        [FromQuery] InvoiceStatus? status)
    {
        var result = await mediator.Send(new GetInvoicesQuery(customerId, status));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDetailResponseModel>> GetInvoice(int id)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceListItemModel>> CreateInvoice(CreateInvoiceRequestModel request)
    {
        var result = await mediator.Send(new CreateInvoiceCommand(
            request.CustomerId, request.SalesOrderId, request.ShipmentId,
            request.InvoiceDate, request.DueDate, request.CreditTerms,
            request.TaxRate, request.Notes, request.Lines));
        return CreatedAtAction(nameof(GetInvoice), new { id = result.Id }, result);
    }

    [HttpPost("{id:int}/send")]
    public async Task<IActionResult> SendInvoice(int id)
    {
        await mediator.Send(new SendInvoiceCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/email")]
    public async Task<IActionResult> EmailInvoice(int id, SendInvoiceEmailRequestModel request)
    {
        await mediator.Send(new SendInvoiceEmailCommand(id, request.RecipientEmail));
        return NoContent();
    }

    [HttpPost("{id:int}/void")]
    public async Task<IActionResult> VoidInvoice(int id)
    {
        await mediator.Send(new VoidInvoiceCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        var pdf = await mediator.Send(new GenerateInvoicePdfQuery(id));
        return File(pdf, "application/pdf", $"invoice-{id}.pdf");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        await mediator.Send(new DeleteInvoiceCommand(id));
        return NoContent();
    }
}
