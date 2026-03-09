using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Inventory;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("locations")]
    public async Task<ActionResult<List<StorageLocationResponseModel>>> GetLocationTree()
    {
        var result = await mediator.Send(new GetLocationTreeQuery());
        return Ok(result);
    }

    [HttpGet("locations/bins")]
    public async Task<ActionResult<List<StorageLocationFlatResponseModel>>> GetBinLocations()
    {
        var result = await mediator.Send(new GetBinLocationsQuery());
        return Ok(result);
    }

    [HttpPost("locations")]
    public async Task<ActionResult<StorageLocationResponseModel>> CreateLocation([FromBody] CreateStorageLocationRequestModel request)
    {
        var result = await mediator.Send(new CreateStorageLocationCommand(request));
        return Created($"/api/v1/inventory/locations", result);
    }

    [HttpGet("locations/{locationId:int}/contents")]
    public async Task<ActionResult<List<BinContentResponseModel>>> GetBinContents(int locationId)
    {
        var result = await mediator.Send(new GetBinContentsQuery(locationId));
        return Ok(result);
    }

    [HttpPost("bin-contents")]
    public async Task<ActionResult<BinContentResponseModel>> PlaceBinContent([FromBody] PlaceBinContentRequestModel request)
    {
        var result = await mediator.Send(new PlaceBinContentCommand(request));
        return Created($"/api/v1/inventory/bin-contents/{result.Id}", result);
    }

    [HttpGet("parts")]
    public async Task<ActionResult<List<InventoryPartSummaryResponseModel>>> GetPartInventory([FromQuery] string? search)
    {
        var result = await mediator.Send(new GetPartInventoryQuery(search));
        return Ok(result);
    }

    [HttpGet("movements")]
    public async Task<ActionResult<List<BinMovementResponseModel>>> GetMovements(
        [FromQuery] int? locationId,
        [FromQuery] string? entityType,
        [FromQuery] int? entityId,
        [FromQuery] int take = 100)
    {
        var result = await mediator.Send(new GetMovementsQuery(locationId, entityType, entityId, take));
        return Ok(result);
    }
}
