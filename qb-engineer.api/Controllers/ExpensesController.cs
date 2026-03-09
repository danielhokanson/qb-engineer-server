using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
