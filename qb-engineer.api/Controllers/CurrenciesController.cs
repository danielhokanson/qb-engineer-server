using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Admin;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/currencies")]
[Authorize(Roles = "Admin")]
public class CurrenciesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CurrencyResponseModel>>> GetCurrencies()
    {
        var result = await mediator.Send(new GetCurrenciesQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CurrencyResponseModel>> CreateCurrency([FromBody] CreateCurrencyRequestModel request)
    {
        var result = await mediator.Send(new CreateCurrencyCommand(request));
        return CreatedAtAction(nameof(GetCurrencies), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCurrency(int id, [FromBody] UpdateCurrencyRequestModel request)
    {
        await mediator.Send(new UpdateCurrencyCommand(id, request));
        return NoContent();
    }
}
