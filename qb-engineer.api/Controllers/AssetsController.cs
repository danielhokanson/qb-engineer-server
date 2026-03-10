using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Activity;
using QBEngineer.Api.Features.Assets;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/assets")]
[Authorize(Roles = "Admin,Manager")]
public class AssetsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AssetResponseModel>>> GetAssets(
        [FromQuery] AssetType? type,
        [FromQuery] AssetStatus? status,
        [FromQuery] string? search)
    {
        var result = await mediator.Send(new GetAssetsQuery(type, status, search));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AssetResponseModel>> CreateAsset([FromBody] CreateAssetRequestModel request)
    {
        var result = await mediator.Send(new CreateAssetCommand(request));
        return Created($"/api/v1/assets/{result.Id}", result);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<AssetResponseModel>> UpdateAsset(int id, [FromBody] UpdateAssetRequestModel request)
    {
        var result = await mediator.Send(new UpdateAssetCommand(id, request));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        await mediator.Send(new DeleteAssetCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetAssetActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Asset", id));
        return Ok(result);
    }
}
