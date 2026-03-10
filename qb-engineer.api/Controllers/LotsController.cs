using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Lots;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/lots")]
[Authorize]
public class LotsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LotRecordResponseModel>>> GetLotRecords(
        [FromQuery] int? partId,
        [FromQuery] int? jobId,
        [FromQuery] string? search)
    {
        var result = await mediator.Send(new GetLotRecordsQuery(partId, jobId, search));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LotRecordResponseModel>> CreateLotRecord(
        [FromBody] CreateLotRecordRequestModel request)
    {
        var result = await mediator.Send(new CreateLotRecordCommand(request));
        return Created($"/api/v1/lots/{result.Id}", result);
    }

    [HttpGet("{lotNumber}/trace")]
    public async Task<ActionResult<LotTraceabilityResponseModel>> GetTraceability(string lotNumber)
    {
        var result = await mediator.Send(new GetLotTraceabilityQuery(lotNumber));
        return Ok(result);
    }
}
