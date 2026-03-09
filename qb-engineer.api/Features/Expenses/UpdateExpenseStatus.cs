using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Expenses;

public record UpdateExpenseStatusCommand(int Id, UpdateExpenseStatusRequestModel Data) : IRequest<ExpenseResponseModel>;

public class UpdateExpenseStatusHandler(IExpenseRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<UpdateExpenseStatusCommand, ExpenseResponseModel>
{
    public async Task<ExpenseResponseModel> Handle(UpdateExpenseStatusCommand request, CancellationToken cancellationToken)
    {
        var expense = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Expense not found.");

        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        expense.Status = request.Data.Status;
        expense.ApprovedBy = userId;
        expense.ApprovalNotes = request.Data.ApprovalNotes?.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetByIdAsync(expense.Id, cancellationToken))!;
    }
}
