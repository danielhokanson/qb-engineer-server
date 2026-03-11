using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Reports;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/report-builder")]
[Authorize]
public class ReportBuilderController(IMediator mediator) : ControllerBase
{
    [HttpGet("entities")]
    public async Task<ActionResult<List<ReportEntityDefinitionModel>>> GetEntities()
    {
        var result = await mediator.Send(new GetReportEntitiesQuery());
        return Ok(result);
    }

    [HttpGet("saved")]
    public async Task<ActionResult<List<SavedReportResponseModel>>> GetSavedReports()
    {
        var result = await mediator.Send(new GetSavedReportsQuery());
        return Ok(result);
    }

    [HttpGet("saved/{id:int}")]
    public async Task<ActionResult<SavedReportResponseModel>> GetSavedReport(int id)
    {
        var result = await mediator.Send(new GetSavedReportQuery(id));
        return Ok(result);
    }

    [HttpPost("saved")]
    public async Task<ActionResult<SavedReportResponseModel>> CreateSavedReport([FromBody] CreateSavedReportRequestModel request)
    {
        var result = await mediator.Send(new CreateSavedReportCommand(
            request.Name,
            request.Description,
            request.EntitySource,
            request.Columns,
            request.Filters,
            request.GroupByField,
            request.SortField,
            request.SortDirection,
            request.ChartType,
            request.ChartLabelField,
            request.ChartValueField,
            request.IsShared));

        return CreatedAtAction(nameof(GetSavedReport), new { id = result.Id }, result);
    }

    [HttpPut("saved/{id:int}")]
    public async Task<ActionResult<SavedReportResponseModel>> UpdateSavedReport(int id, [FromBody] UpdateSavedReportRequestModel request)
    {
        var result = await mediator.Send(new UpdateSavedReportCommand(
            id,
            request.Name,
            request.Description,
            request.EntitySource,
            request.Columns,
            request.Filters,
            request.GroupByField,
            request.SortField,
            request.SortDirection,
            request.ChartType,
            request.ChartLabelField,
            request.ChartValueField,
            request.IsShared));

        return Ok(result);
    }

    [HttpDelete("saved/{id:int}")]
    public async Task<IActionResult> DeleteSavedReport(int id)
    {
        await mediator.Send(new DeleteSavedReportCommand(id));
        return NoContent();
    }

    [HttpPost("run")]
    public async Task<ActionResult<RunReportResponseModel>> RunReport([FromBody] RunReportRequestModel request)
    {
        var result = await mediator.Send(new RunReportCommand(
            request.EntitySource,
            request.Columns,
            request.Filters,
            request.GroupByField,
            request.SortField,
            request.SortDirection,
            request.Page,
            request.PageSize));

        return Ok(result);
    }
}
