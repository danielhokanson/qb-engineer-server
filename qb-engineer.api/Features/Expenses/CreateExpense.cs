using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Expenses;

public record CreateExpenseCommand(CreateExpenseRequestModel Data) : IRequest<ExpenseResponseModel>;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Data.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
        RuleFor(x => x.Data.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Data.Description).NotEmpty().MaximumLength(500);
    }
}

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
