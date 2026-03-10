using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Quality;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/quality")]
[Authorize]
public class QualityController(IMediator mediator) : ControllerBase
{
    [HttpGet("templates")]
    public async Task<ActionResult<List<QcTemplateResponseModel>>> GetTemplates()
    {
        var result = await mediator.Send(new GetQcTemplatesQuery());
        return Ok(result);
    }

    [HttpPost("templates")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<QcTemplateResponseModel>> CreateTemplate(
        [FromBody] CreateQcTemplateRequestModel request)
    {
        var result = await mediator.Send(new CreateQcTemplateCommand(request));
        return Created($"/api/v1/quality/templates/{result.Id}", result);
    }

    [HttpGet("inspections")]
    public async Task<ActionResult<List<QcInspectionResponseModel>>> GetInspections(
        [FromQuery] int? jobId,
        [FromQuery] string? status,
        [FromQuery] string? lotNumber)
    {
        var result = await mediator.Send(new GetQcInspectionsQuery(jobId, status, lotNumber));
        return Ok(result);
    }

    [HttpPost("inspections")]
    public async Task<ActionResult<QcInspectionResponseModel>> CreateInspection(
        [FromBody] CreateQcInspectionRequestModel request)
    {
        var result = await mediator.Send(new CreateQcInspectionCommand(request));
        return Created($"/api/v1/quality/inspections/{result.Id}", result);
    }

    [HttpPut("inspections/{id:int}")]
    public async Task<ActionResult<QcInspectionResponseModel>> UpdateInspection(
        int id, [FromBody] UpdateQcInspectionRequestModel request)
    {
        var result = await mediator.Send(new UpdateQcInspectionCommand(id, request));
        return Ok(result);
    }
}
