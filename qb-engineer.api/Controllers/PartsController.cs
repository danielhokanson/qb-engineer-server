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
[Authorize(Roles = "Admin,Manager,Engineer,ProductionWorker,PM,OfficeManager")]
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

    [HttpGet("{id:int}/operations")]
    public async Task<ActionResult<List<OperationResponseModel>>> GetOperations(int id)
        => Ok(await mediator.Send(new GetOperationsQuery(id)));

    [HttpPost("{id:int}/operations")]
    public async Task<ActionResult<OperationResponseModel>> CreateOperation(int id, [FromBody] CreateOperationRequestModel request)
        => StatusCode(201, await mediator.Send(new CreateOperationCommand(id, request)));

    [HttpPatch("{id:int}/operations/{operationId:int}")]
    public async Task<ActionResult<OperationResponseModel>> UpdateOperation(int id, int operationId, [FromBody] UpdateOperationRequestModel request)
        => Ok(await mediator.Send(new UpdateOperationCommand(id, operationId, request)));

    [HttpDelete("{id:int}/operations/{operationId:int}")]
    public async Task<IActionResult> DeleteOperation(int id, int operationId)
    {
        await mediator.Send(new DeleteOperationCommand(id, operationId));
        return NoContent();
    }

    [HttpPost("{id:int}/operations/{operationId:int}/materials")]
    public async Task<ActionResult<OperationMaterialResponseModel>> CreateOperationMaterial(int id, int operationId, [FromBody] CreateOperationMaterialRequestModel request)
        => StatusCode(201, await mediator.Send(new CreateOperationMaterialCommand(id, operationId, request)));

    [HttpDelete("{id:int}/operations/{operationId:int}/materials/{materialId:int}")]
    public async Task<IActionResult> DeleteOperationMaterial(int id, int operationId, int materialId)
    {
        await mediator.Send(new DeleteOperationMaterialCommand(id, operationId, materialId));
        return NoContent();
    }

    [HttpGet("{id:int}/operations/{operationId:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetOperationActivity(int id, int operationId)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Operation", operationId));
        return Ok(result);
    }

    [HttpPost("{id:int}/operations/{operationId:int}/activity")]
    public async Task<IActionResult> AddOperationComment(int id, int operationId, [FromBody] AddOperationCommentRequestModel request)
    {
        await mediator.Send(new AddOperationCommentCommand(id, operationId, request.Comment));
        return StatusCode(201);
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

    [HttpGet("thumbnails")]
    public async Task<ActionResult<List<PartThumbnailResponseModel>>> GetThumbnails([FromQuery] List<int> partIds)
    {
        var result = await mediator.Send(new GetPartThumbnailsQuery(partIds));
        return Ok(result);
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetPartActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Part", id));
        return Ok(result);
    }

    // ── Pricing ───────────────────────────────────────────────────────────────

    [HttpGet("{id:int}/prices")]
    public async Task<ActionResult<List<PartPriceResponseModel>>> GetPrices(int id)
    {
        var result = await mediator.Send(new GetPartPricesQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/prices")]
    public async Task<ActionResult<PartPriceResponseModel>> AddPrice(
        int id, [FromBody] AddPartPriceRequestModel model)
    {
        var result = await mediator.Send(new AddPartPriceCommand(id, model.UnitPrice, model.EffectiveFrom, model.Notes));
        return CreatedAtAction(nameof(GetPrices), new { id }, result);
    }

    // ── Alternates ───────────────────────────────────────────────────────────

    [HttpGet("{id:int}/alternates")]
    public async Task<ActionResult<List<PartAlternateResponseModel>>> GetAlternates(int id)
    {
        var result = await mediator.Send(new GetPartAlternatesQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/alternates")]
    public async Task<ActionResult<PartAlternateResponseModel>> CreateAlternate(
        int id, [FromBody] CreatePartAlternateRequestModel request)
    {
        var result = await mediator.Send(new CreatePartAlternateCommand(id, request));
        return Created($"/api/v1/parts/{id}/alternates/{result.Id}", result);
    }

    [HttpPatch("{id:int}/alternates/{alternateId:int}")]
    public async Task<ActionResult<PartAlternateResponseModel>> UpdateAlternate(
        int id, int alternateId, [FromBody] UpdatePartAlternateRequestModel request)
    {
        var result = await mediator.Send(new UpdatePartAlternateCommand(id, alternateId, request));
        return Ok(result);
    }

    [HttpDelete("{id:int}/alternates/{alternateId:int}")]
    public async Task<IActionResult> DeleteAlternate(int id, int alternateId)
    {
        await mediator.Send(new DeletePartAlternateCommand(id, alternateId));
        return NoContent();
    }
}
