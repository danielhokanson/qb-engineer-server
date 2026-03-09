using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Reports;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet("jobs-by-stage")]
    public async Task<ActionResult<List<JobsByStageReportItem>>> GetJobsByStage([FromQuery] int? trackTypeId)
    {
        var result = await mediator.Send(new GetJobsByStageReportQuery(trackTypeId));
        return Ok(result);
    }

    [HttpGet("overdue-jobs")]
    public async Task<ActionResult<List<OverdueJobReportItem>>> GetOverdueJobs()
    {
        var result = await mediator.Send(new GetOverdueJobsReportQuery());
        return Ok(result);
    }

    [HttpGet("time-by-user")]
    public async Task<ActionResult<List<TimeByUserReportItem>>> GetTimeByUser(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetTimeByUserReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("expense-summary")]
    public async Task<ActionResult<List<ExpenseSummaryReportItem>>> GetExpenseSummary(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetExpenseSummaryReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("lead-pipeline")]
    public async Task<ActionResult<List<LeadPipelineReportItem>>> GetLeadPipeline()
    {
        var result = await mediator.Send(new GetLeadPipelineReportQuery());
        return Ok(result);
    }

    [HttpGet("job-completion-trend")]
    public async Task<ActionResult<List<JobCompletionTrendItem>>> GetJobCompletionTrend([FromQuery] int months = 6)
    {
        var result = await mediator.Send(new GetJobCompletionTrendQuery(months));
        return Ok(result);
    }
}
