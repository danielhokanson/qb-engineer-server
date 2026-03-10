using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Activity;
using QBEngineer.Api.Features.Expenses;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public class ExpensesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExpenseResponseModel>>> GetExpenses(
        [FromQuery] int? userId,
        [FromQuery] ExpenseStatus? status,
        [FromQuery] string? search)
    {
        var result = await mediator.Send(new GetExpensesQuery(userId, status, search));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseModel>> CreateExpense([FromBody] CreateExpenseRequestModel request)
    {
        var result = await mediator.Send(new CreateExpenseCommand(request));
        return Created($"/api/v1/expenses/{result.Id}", result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ExpenseResponseModel>> UpdateExpenseStatus(int id, [FromBody] UpdateExpenseStatusRequestModel request)
    {
        var result = await mediator.Send(new UpdateExpenseStatusCommand(id, request));
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        await mediator.Send(new DeleteExpenseCommand(id));
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityResponseModel>>> GetExpenseActivity(int id)
    {
        var result = await mediator.Send(new GetEntityActivityQuery("Expense", id));
        return Ok(result);
    }

    [HttpGet("settings")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ExpenseSettingsResponse>> GetSettings()
    {
        var result = await mediator.Send(new GetExpenseSettingsQuery());
        return Ok(result);
    }

    [HttpPut("settings")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateExpenseSettingsCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }
}
