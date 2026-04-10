using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Activity;
using QBEngineer.Api.Features.Customers;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize(Roles = "Admin,Manager,OfficeManager,PM")]
public class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CustomerListItemModel>>> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] bool? isActive)
    {
        var result = await mediator.Send(new GetCustomerListQuery(search, isActive));
        return Ok(result);
    }

    [HttpGet("dropdown")]
    public async Task<ActionResult<List<CustomerResponseModel>>> GetCustomerDropdown()
    {
        var result = await mediator.Send(new GetCustomersQuery());
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDetailResponseModel>> GetCustomer(int id)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerListItemModel>> CreateCustomer(CreateCustomerRequestModel request)
    {
        var result = await mediator.Send(new CreateCustomerCommand(
            request.Name, request.CompanyName, request.Email, request.Phone));
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerRequestModel request)
    {
        await mediator.Send(new UpdateCustomerCommand(
            id, request.Name, request.CompanyName, request.Email, request.Phone, request.IsActive));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }

    [HttpPost("{id:int}/contacts")]
    public async Task<ActionResult<ContactResponseModel>> CreateContact(int id, CreateContactRequestModel request)
    {
        var result = await mediator.Send(new CreateContactCommand(
            id, request.FirstName, request.LastName, request.Email, request.Phone, request.Role, request.IsPrimary));
        return Created($"/api/v1/customers/{id}/contacts/{result.Id}", result);
    }

    [HttpPut("{id:int}/contacts/{contactId:int}")]
    public async Task<ActionResult<ContactResponseModel>> UpdateContact(int id, int contactId, UpdateContactRequestModel request)
    {
        var result = await mediator.Send(new UpdateContactCommand(
            id, contactId, request.FirstName, request.LastName, request.Email, request.Phone, request.Role, request.IsPrimary));
        return Ok(result);
    }

    [HttpDelete("{id:int}/contacts/{contactId:int}")]
    public async Task<IActionResult> DeleteContact(int id, int contactId)
    {
        await mediator.Send(new DeleteContactCommand(id, contactId));
        return NoContent();
    }

    // ─── Contact Interactions ───

    [HttpGet("{id:int}/interactions")]
    public async Task<ActionResult<List<ContactInteractionResponseModel>>> GetInteractions(
        int id, [FromQuery] int? contactId)
    {
        var result = await mediator.Send(new GetContactInteractionsQuery(id, contactId));
        return Ok(result);
    }

    [HttpPost("{id:int}/interactions")]
    public async Task<ActionResult<ContactInteractionResponseModel>> CreateInteraction(
        int id, [FromBody] ContactInteractionRequestModel request)
    {
        var result = await mediator.Send(new CreateContactInteractionCommand(
            id, request.ContactId, request.Type, request.Subject,
            request.Body, request.InteractionDate, request.DurationMinutes));
        return Created($"/api/v1/customers/{id}/interactions/{result.Id}", result);
    }

    [HttpPatch("{id:int}/interactions/{interactionId:int}")]
    public async Task<ActionResult<ContactInteractionResponseModel>> UpdateInteraction(
        int id, int interactionId, [FromBody] ContactInteractionRequestModel request)
    {
        var result = await mediator.Send(new UpdateContactInteractionCommand(
            id, interactionId, request.Type, request.Subject,
            request.Body, request.InteractionDate, request.DurationMinutes));
        return Ok(result);
    }

    [HttpDelete("{id:int}/interactions/{interactionId:int}")]
    public async Task<IActionResult> DeleteInteraction(int id, int interactionId)
    {
        await mediator.Send(new DeleteContactInteractionCommand(id, interactionId));
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetCustomerActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Customer", id));
        return Ok(result);
    }

    [HttpGet("{id:int}/statement")]
    public async Task<IActionResult> GetStatement(int id)
    {
        var pdf = await mediator.Send(new GenerateCustomerStatementQuery(id));
        return File(pdf, "application/pdf", $"statement-{id}.pdf");
    }

    [HttpGet("{id:int}/summary")]
    public async Task<ActionResult<CustomerSummaryResponseModel>> GetSummary(int id, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCustomerSummaryQuery(id), ct);
        return Ok(result);
    }
}
