using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Expenses;

public record UpdateExpenseSettingsCommand(bool AllowSelfApproval, decimal? AutoApproveThreshold) : IRequest;

public class UpdateExpenseSettingsValidator : AbstractValidator<UpdateExpenseSettingsCommand>
{
    public UpdateExpenseSettingsValidator()
    {
        RuleFor(x => x.AutoApproveThreshold)
            .GreaterThan(0).When(x => x.AutoApproveThreshold.HasValue)
            .WithMessage("Auto-approve threshold must be greater than 0.");
    }
}

public class UpdateExpenseSettingsHandler(AppDbContext db) : IRequestHandler<UpdateExpenseSettingsCommand>
{
    public async Task Handle(UpdateExpenseSettingsCommand request, CancellationToken ct)
    {
        await UpsertSetting("expense_self_approval", request.AllowSelfApproval.ToString().ToLower(),
            "Allow users to approve their own expenses", ct);

        if (request.AutoApproveThreshold.HasValue)
            await UpsertSetting("expense_auto_approve_threshold", request.AutoApproveThreshold.Value.ToString("F2"),
                "Expenses below this amount are auto-approved", ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task UpsertSetting(string key, string value, string description, CancellationToken ct)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
            await db.SystemSettings.AddAsync(new SystemSetting { Key = key, Value = value, Description = description }, ct);
        else
            setting.Value = value;
    }
}
