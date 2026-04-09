using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Reports;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = "Admin,Manager,OfficeManager,PM")]
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

    // ─── Financial Reports ───

    [HttpGet("ar-aging")]
    public async Task<ActionResult<List<ArAgingReportItem>>> GetArAging()
    {
        var result = await mediator.Send(new GetArAgingReportQuery());
        return Ok(result);
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<List<RevenueReportItem>>> GetRevenue(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end,
        [FromQuery] string groupBy = "period")
    {
        var result = await mediator.Send(new GetRevenueReportQuery(start, end, groupBy));
        return Ok(result);
    }

    [HttpGet("simple-pnl")]
    public async Task<ActionResult<List<SimplePnlReportItem>>> GetSimplePnl(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetSimplePnlReportQuery(start, end));
        return Ok(result);
    }

    // ─── Additional Reports ───

    [HttpGet("my-expense-history")]
    public async Task<ActionResult<List<MyExpenseHistoryReportItem>>> GetMyExpenseHistory(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetMyExpenseHistoryQuery(start, end));
        return Ok(result);
    }

    [HttpGet("quote-to-close")]
    public async Task<ActionResult<List<QuoteToCloseReportItem>>> GetQuoteToClose(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetQuoteToCloseReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("shipping-summary")]
    public async Task<ActionResult<List<ShippingSummaryReportItem>>> GetShippingSummary(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetShippingSummaryReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("time-in-stage")]
    public async Task<ActionResult<List<TimeInStageReportItem>>> GetTimeInStage([FromQuery] int? trackTypeId)
    {
        var result = await mediator.Send(new GetTimeInStageReportQuery(trackTypeId));
        return Ok(result);
    }

    // ─── Batch 4 Reports ───

    [HttpGet("employee-productivity")]
    public async Task<ActionResult<List<EmployeeProductivityReportItem>>> GetEmployeeProductivity(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetEmployeeProductivityReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("inventory-levels")]
    public async Task<ActionResult<List<InventoryLevelReportItem>>> GetInventoryLevels()
    {
        var result = await mediator.Send(new GetInventoryLevelsReportQuery());
        return Ok(result);
    }

    [HttpGet("maintenance")]
    public async Task<ActionResult<List<MaintenanceReportItem>>> GetMaintenance(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetMaintenanceReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("quality-scrap")]
    public async Task<ActionResult<List<QualityScrapReportItem>>> GetQualityScrap(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetQualityScrapReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("cycle-review")]
    public async Task<ActionResult<List<CycleReviewReportItem>>> GetCycleReview()
    {
        var result = await mediator.Send(new GetCycleReviewReportQuery());
        return Ok(result);
    }

    [HttpGet("job-margin")]
    public async Task<ActionResult<List<JobMarginReportItem>>> GetJobMargin(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetJobMarginReportQuery(start, end));
        return Ok(result);
    }

    // ─── Batch 5 Reports ───

    [HttpGet("my-cycle-summary")]
    public async Task<ActionResult<List<MyCycleSummaryReportItem>>> GetMyCycleSummary()
    {
        var result = await mediator.Send(new GetMyCycleSummaryReportQuery());
        return Ok(result);
    }

    [HttpGet("lead-sales")]
    public async Task<ActionResult<LeadSalesReportItem>> GetLeadSales(
        [FromQuery] DateTimeOffset start, [FromQuery] DateTimeOffset end)
    {
        var result = await mediator.Send(new GetLeadSalesReportQuery(start, end));
        return Ok(result);
    }

    [HttpGet("rd")]
    public async Task<ActionResult<List<RdReportItem>>> GetRdReport(
        [FromQuery] DateTimeOffset? start, [FromQuery] DateTimeOffset? end)
    {
        var result = await mediator.Send(new GetRdReportQuery(start, end));
        return Ok(result);
    }
}
