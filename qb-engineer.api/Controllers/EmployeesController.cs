using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Activity;
using QBEngineer.Api.Features.Employees;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
[Authorize(Roles = "Admin,Manager")]
public class EmployeesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EmployeeListItemResponseModel>>> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] int? teamId,
        [FromQuery] string? role,
        [FromQuery] bool? isActive)
    {
        var callerUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var callerIsAdmin = User.IsInRole("Admin");

        var result = await mediator.Send(new GetEmployeeListQuery(search, teamId, role, isActive, callerUserId, callerIsAdmin));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDetailResponseModel>> GetEmployee(int id)
    {
        var result = await mediator.Send(new GetEmployeeDetailQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<EmployeeStatsResponseModel>> GetEmployeeStats(int id)
    {
        var result = await mediator.Send(new GetEmployeeStatsQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/time-summary")]
    public async Task<ActionResult<List<EmployeeTimeEntryItem>>> GetTimeSummary(int id, [FromQuery] string? period)
    {
        var result = await mediator.Send(new GetEmployeeTimeSummaryQuery(id, period));
        return Ok(result);
    }

    [HttpGet("{id:int}/pay-summary")]
    public async Task<ActionResult<List<EmployeePayStubItem>>> GetPaySummary(int id)
    {
        var result = await mediator.Send(new GetEmployeePaySummaryQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/jobs")]
    public async Task<ActionResult<List<EmployeeJobItem>>> GetJobs(int id)
    {
        var result = await mediator.Send(new GetEmployeeJobsQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/expenses")]
    public async Task<ActionResult<List<EmployeeExpenseItem>>> GetExpenses(int id)
    {
        var result = await mediator.Send(new GetEmployeeExpensesQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/training")]
    public async Task<ActionResult<List<EmployeeTrainingItem>>> GetTraining(int id)
    {
        var result = await mediator.Send(new GetEmployeeTrainingQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/compliance")]
    public async Task<ActionResult<List<EmployeeComplianceItem>>> GetCompliance(int id)
    {
        var result = await mediator.Send(new GetEmployeeComplianceQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityEntryResponseModel>>> GetActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Employee", id));
        return Ok(result);
    }
}
