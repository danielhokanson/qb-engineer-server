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
            request.Description, request.Revision,
            request.PartType, request.Material, request.MoldToolRef, request.ExternalPartNumber));
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

    [HttpGet("{id:int}/revisions")]
    public async Task<ActionResult<List<PartRevisionResponseModel>>> GetPartRevisions(int id)
    {
        var result = await mediator.Send(new GetPartRevisionsQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/revisions")]
    public async Task<ActionResult<PartRevisionResponseModel>> CreatePartRevision(int id, [FromBody] CreatePartRevisionRequestModel request)
    {
        var result = await mediator.Send(new CreatePartRevisionCommand(
            id, request.Revision, request.ChangeDescription, request.ChangeReason, request.EffectiveDate));
        return Created($"/api/v1/parts/{id}/revisions/{result.Id}", result);
    }

    [HttpGet("{id:int}/process-steps")]
    public async Task<ActionResult<List<ProcessStepResponseModel>>> GetProcessSteps(int id)
        => Ok(await mediator.Send(new GetProcessStepsQuery(id)));

    [HttpPost("{id:int}/process-steps")]
    public async Task<ActionResult<ProcessStepResponseModel>> CreateProcessStep(int id, [FromBody] CreateProcessStepRequestModel request)
        => StatusCode(201, await mediator.Send(new CreateProcessStepCommand(id, request)));

    [HttpPatch("{id:int}/process-steps/{stepId:int}")]
    public async Task<ActionResult<ProcessStepResponseModel>> UpdateProcessStep(int id, int stepId, [FromBody] UpdateProcessStepRequestModel request)
        => Ok(await mediator.Send(new UpdateProcessStepCommand(id, stepId, request)));

    [HttpDelete("{id:int}/process-steps/{stepId:int}")]
    public async Task<IActionResult> DeleteProcessStep(int id, int stepId)
    {
        await mediator.Send(new DeleteProcessStepCommand(id, stepId));
        return NoContent();
    }

    [HttpPost("{id:int}/link-accounting-item")]
    public async Task<IActionResult> LinkAccountingItem(int id, [FromBody] LinkAccountingItemRequestModel request)
    {
        await mediator.Send(new LinkPartToAccountingItemCommand(id, request.ExternalId, request.ExternalRef));
        return NoContent();
    }

    [HttpDelete("{id:int}/link-accounting-item")]
    public async Task<IActionResult> UnlinkAccountingItem(int id)
    {
        await mediator.Send(new UnlinkPartFromAccountingItemCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetPartActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Part", id));
        return Ok(result);
    }
}
