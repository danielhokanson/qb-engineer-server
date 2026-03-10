using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Expenses;

public record UpdateExpenseStatusCommand(int Id, UpdateExpenseStatusRequestModel Data) : IRequest<ExpenseResponseModel>;

public class UpdateExpenseStatusValidator : AbstractValidator<UpdateExpenseStatusCommand>
{
    public UpdateExpenseStatusValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Data.Status).IsInEnum();
        RuleFor(x => x.Data.ApprovalNotes).MaximumLength(1000).When(x => x.Data.ApprovalNotes is not null);
    }
}

public class UpdateExpenseStatusHandler(IExpenseRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<UpdateExpenseStatusCommand, ExpenseResponseModel>
{
    public async Task<ExpenseResponseModel> Handle(UpdateExpenseStatusCommand request, CancellationToken cancellationToken)
    {
        var expense = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Expense not found.");

        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        expense.Status = request.Data.Status;
        expense.ApprovedBy = userId;
        expense.ApprovalNotes = request.Data.ApprovalNotes?.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetByIdAsync(expense.Id, cancellationToken))!;
    }
}
