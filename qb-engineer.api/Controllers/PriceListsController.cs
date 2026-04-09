using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.PriceLists;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/price-lists")]
[Authorize(Roles = "Admin,Manager,OfficeManager")]
public class PriceListsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PriceListListItemModel>>> GetPriceLists(
        [FromQuery] int? customerId)
    {
        var result = await mediator.Send(new GetPriceListsQuery(customerId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriceListResponseModel>> GetPriceList(int id)
    {
        var result = await mediator.Send(new GetPriceListByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PriceListListItemModel>> CreatePriceList(CreatePriceListRequestModel request)
    {
        var result = await mediator.Send(new CreatePriceListCommand(
            request.Name, request.Description, request.CustomerId,
            request.IsDefault, request.EffectiveFrom, request.EffectiveTo,
            request.Entries));
        return CreatedAtAction(nameof(GetPriceList), new { id = result.Id }, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePriceList(int id)
    {
        await mediator.Send(new DeletePriceListCommand(id));
        return NoContent();
    }
}
