using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Vendors;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/vendors")]
[Authorize]
public class VendorsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VendorListItemModel>>> GetVendors(
        [FromQuery] string? search,
        [FromQuery] bool? isActive)
    {
        var result = await mediator.Send(new GetVendorsQuery(search, isActive));
        return Ok(result);
    }

    [HttpGet("dropdown")]
    public async Task<ActionResult<List<VendorResponseModel>>> GetVendorDropdown()
    {
        var result = await mediator.Send(new GetVendorDropdownQuery());
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendorDetailResponseModel>> GetVendor(int id)
    {
        var result = await mediator.Send(new GetVendorByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<VendorListItemModel>> CreateVendor(CreateVendorRequestModel request)
    {
        var result = await mediator.Send(new CreateVendorCommand(
            request.CompanyName, request.ContactName, request.Email, request.Phone,
            request.Address, request.City, request.State, request.ZipCode,
            request.Country, request.PaymentTerms, request.Notes));
        return CreatedAtAction(nameof(GetVendor), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVendor(int id, UpdateVendorRequestModel request)
    {
        await mediator.Send(new UpdateVendorCommand(
            id, request.CompanyName, request.ContactName, request.Email, request.Phone,
            request.Address, request.City, request.State, request.ZipCode,
            request.Country, request.PaymentTerms, request.Notes, request.IsActive));
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        await mediator.Send(new DeleteVendorCommand(id));
        return NoContent();
    }
}
