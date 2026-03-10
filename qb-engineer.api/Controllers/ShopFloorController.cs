using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.ShopFloor;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/display/shop-floor")]
[AllowAnonymous]
public class ShopFloorController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ShopFloorOverviewResponseModel>> GetOverview()
    {
        var result = await mediator.Send(new GetShopFloorOverviewQuery());
        return Ok(result);
    }
}
