using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Activity;
using QBEngineer.Api.Features.Parts;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/parts")]
[Authorize]
public class PartsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PartListResponseModel>>> GetParts(
        [FromQuery] PartStatus? status,
        [FromQuery] PartType? type,
        [FromQuery] string? search)
    {
        var result = await mediator.Send(new GetPartsQuery(status, type, search));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartDetailResponseModel>> GetPartById(int id)
    {
        var result = await mediator.Send(new GetPartByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PartDetailResponseModel>> CreatePart([FromBody] CreatePartRequestModel request)
    {
        var result = await mediator.Send(new CreatePartCommand(
            request.PartNumber, request.Description, request.Revision,
            request.PartType, request.Material, request.MoldToolRef));
        return Created($"/api/v1/parts/{result.Id}", result);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<PartDetailResponseModel>> UpdatePart(int id, [FromBody] UpdatePartRequestModel request)
    {
        var result = await mediator.Send(new UpdatePartCommand(id, request));
        return Ok(result);
    }

    [HttpPost("{id:int}/bom")]
    public async Task<ActionResult<PartDetailResponseModel>> CreateBOMEntry(int id, [FromBody] CreateBOMEntryRequestModel request)
    {
        var result = await mediator.Send(new CreateBOMEntryCommand(id, request));
        return Created($"/api/v1/parts/{id}", result);
    }

    [HttpPatch("{id:int}/bom/{bomEntryId:int}")]
    public async Task<ActionResult<PartDetailResponseModel>> UpdateBOMEntry(int id, int bomEntryId, [FromBody] UpdateBOMEntryRequestModel request)
    {
        var result = await mediator.Send(new UpdateBOMEntryCommand(id, bomEntryId, request));
        return Ok(result);
    }

    [HttpDelete("{id:int}/bom/{bomEntryId:int}")]
    public async Task<ActionResult<PartDetailResponseModel>> DeleteBOMEntry(int id, int bomEntryId)
    {
        var result = await mediator.Send(new DeleteBOMEntryCommand(id, bomEntryId));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        await mediator.Send(new DeletePartCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetPartActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Part", id));
        return Ok(result);
    }
}
