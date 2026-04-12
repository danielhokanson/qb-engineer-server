using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Quality;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/reports/copq")]
[Authorize]
public class CopqController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CopqReportResponseModel>> GetReport(
        [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await mediator.Send(new GetCopqReportQuery(startDate, endDate));
        return Ok(result);
    }

    [HttpGet("trend")]
    public async Task<ActionResult<IReadOnlyList<CopqTrendPointResponseModel>>> GetTrend(
        [FromQuery] int months = 12)
    {
        var result = await mediator.Send(new GetCopqTrendQuery(months));
        return Ok(result);
    }

    [HttpGet("pareto")]
    public async Task<ActionResult<IReadOnlyList<CopqParetoItemResponseModel>>> GetPareto(
        [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await mediator.Send(new GetCopqParetoQuery(startDate, endDate));
        return Ok(result);
    }
}
