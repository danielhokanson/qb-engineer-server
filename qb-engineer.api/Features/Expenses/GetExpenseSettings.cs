using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Expenses;

public record GetExpenseSettingsQuery : IRequest<ExpenseSettingsResponse>;

public record ExpenseSettingsResponse(bool AllowSelfApproval, decimal? AutoApproveThreshold);

public class GetExpenseSettingsHandler(AppDbContext db) : IRequestHandler<GetExpenseSettingsQuery, ExpenseSettingsResponse>
{
    public async Task<ExpenseSettingsResponse> Handle(GetExpenseSettingsQuery request, CancellationToken ct)
    {
        var selfApproval = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "expense_self_approval", ct);
        var threshold = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "expense_auto_approve_threshold", ct);

        return new ExpenseSettingsResponse(
            selfApproval?.Value == "true",
            threshold is not null && decimal.TryParse(threshold.Value, out var t) ? t : null);
    }
}
