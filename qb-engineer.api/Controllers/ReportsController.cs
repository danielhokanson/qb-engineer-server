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

    [HttpGet("on-time-delivery")]
    public async Task<ActionResult<OnTimeDeliveryReportItem>> GetOnTimeDelivery(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetOnTimeDeliveryReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("average-lead-time")]
    public async Task<ActionResult<List<AverageLeadTimeReportItem>>> GetAverageLeadTime()
    {
        var result = await mediator.Send(new GetAverageLeadTimeReportQuery());
        return Ok(result);
    }

    [HttpGet("team-workload")]
    public async Task<ActionResult<List<TeamWorkloadReportItem>>> GetTeamWorkload()
    {
        var result = await mediator.Send(new GetTeamWorkloadReportQuery());
        return Ok(result);
    }

    [HttpGet("customer-activity")]
    public async Task<ActionResult<List<CustomerActivityReportItem>>> GetCustomerActivity()
    {
        var result = await mediator.Send(new GetCustomerActivityReportQuery());
        return Ok(result);
    }

    [HttpGet("my-work-history")]
    public async Task<ActionResult<List<MyWorkHistoryReportItem>>> GetMyWorkHistory()
    {
        var result = await mediator.Send(new GetMyWorkHistoryQuery());
        return Ok(result);
    }

    [HttpGet("my-time-log")]
    public async Task<ActionResult<List<MyTimeLogReportItem>>> GetMyTimeLog(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetMyTimeLogQuery(start, end));
        return Ok(result);
    }
}
