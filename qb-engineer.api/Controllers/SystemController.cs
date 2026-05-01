using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Endpoints exposing install-wide configuration the UI needs at boot.
/// Currently just the base currency; expand here as other "tell the UI
/// what's configured" needs surface.
/// </summary>
[ApiController]
[Route("api/v1/system")]
[Authorize]
public class SystemController(ICurrencyService currencyService) : ControllerBase
{
    /// <summary>
    /// Returns the install's base currency so the UI can decide whether
    /// to suffix record-level ISO codes when displaying prices.
    /// </summary>
    [HttpGet("currency-base")]
    public async Task<ActionResult<CurrencyBaseResponseModel>> GetCurrencyBase(CancellationToken ct)
    {
        var code = await currencyService.GetBaseCurrencyAsync(ct);
        return Ok(new CurrencyBaseResponseModel(code));
    }
}
