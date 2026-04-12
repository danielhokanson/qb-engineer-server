using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Admin;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/exchange-rates")]
[Authorize(Roles = "Admin")]
public class ExchangeRatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateResponseModel>>> GetExchangeRates(
        [FromQuery] int? fromCurrencyId = null,
        [FromQuery] int? toCurrencyId = null,
        [FromQuery] DateOnly? dateFrom = null,
        [FromQuery] DateOnly? dateTo = null)
    {
        var result = await mediator.Send(new GetExchangeRatesQuery(fromCurrencyId, toCurrencyId, dateFrom, dateTo));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExchangeRateResponseModel>> SetExchangeRate([FromBody] SetExchangeRateRequestModel request)
    {
        var result = await mediator.Send(new SetExchangeRateCommand(request));
        return CreatedAtAction(nameof(GetExchangeRates), new { }, result);
    }

    [HttpGet("convert")]
    public async Task<ActionResult<object>> Convert(
        [FromQuery] int fromCurrencyId,
        [FromQuery] int toCurrencyId,
        [FromQuery] decimal amount,
        [FromQuery] DateOnly date)
    {
        var converted = await mediator.Send(new ConvertCurrencyQuery(
            new ConvertCurrencyRequestModel(fromCurrencyId, toCurrencyId, amount, date)));
        return Ok(new { convertedAmount = converted });
    }
}
