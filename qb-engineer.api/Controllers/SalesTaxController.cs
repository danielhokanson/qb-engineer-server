using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.SalesTax;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/sales-tax-rates")]
[Authorize]
public class SalesTaxController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SalesTaxRateResponseModel>>> GetAll()
    {
        var result = await mediator.Send(new GetSalesTaxRatesQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SalesTaxRateResponseModel>> Create([FromBody] CreateSalesTaxRateRequestModel request)
    {
        var result = await mediator.Send(new CreateSalesTaxRateCommand(request));
        return Created($"/api/v1/sales-tax-rates/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<SalesTaxRateResponseModel>> Update(int id, [FromBody] CreateSalesTaxRateRequestModel request)
    {
        var result = await mediator.Send(new UpdateSalesTaxRateCommand(id, request));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await mediator.Send(new DeleteSalesTaxRateCommand(id));
        return NoContent();
    }
}
