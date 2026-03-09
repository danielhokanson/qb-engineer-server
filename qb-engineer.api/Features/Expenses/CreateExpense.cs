using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Expenses;

public record CreateExpenseCommand(CreateExpenseRequestModel Data) : IRequest<ExpenseResponseModel>;

public class CreateExpenseHandler(IExpenseRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<CreateExpenseCommand, ExpenseResponseModel>
{
    public async Task<ExpenseResponseModel> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var expense = new Expense
        {
            UserId = userId,
            JobId = data.JobId,
            Amount = data.Amount,
            Category = data.Category.Trim(),
            Description = data.Description.Trim(),
            ReceiptFileId = data.ReceiptFileId,
            ExpenseDate = data.ExpenseDate,
        };

        await repo.AddAsync(expense, cancellationToken);

        return (await repo.GetByIdAsync(expense.Id, cancellationToken))!;
    }
}
