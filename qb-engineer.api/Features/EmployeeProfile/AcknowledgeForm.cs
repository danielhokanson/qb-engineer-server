using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.EmployeeProfile;

public record AcknowledgeFormCommand(int UserId, string FormType) : IRequest;

public class AcknowledgeFormHandler(AppDbContext db) : IRequestHandler<AcknowledgeFormCommand>
{
    private static readonly HashSet<string> ValidFormTypes =
    [
        "w4", "state_withholding", "i9", "direct_deposit", "workers_comp", "handbook"
    ];

    public async Task Handle(AcknowledgeFormCommand request, CancellationToken ct)
    {
        if (!ValidFormTypes.Contains(request.FormType))
            throw new ArgumentException($"Invalid form type: {request.FormType}");

        var profile = await db.EmployeeProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        if (profile is null)
        {
            profile = new Core.Entities.EmployeeProfile { UserId = request.UserId };
            db.EmployeeProfiles.Add(profile);
        }

        var now = DateTime.UtcNow;

        switch (request.FormType)
        {
            case "w4":
                profile.W4CompletedAt = now;
                break;
            case "state_withholding":
                profile.StateWithholdingCompletedAt = now;
                break;
            case "i9":
                profile.I9CompletedAt = now;
                break;
            case "direct_deposit":
                profile.DirectDepositCompletedAt = now;
                break;
            case "workers_comp":
                profile.WorkersCompAcknowledgedAt = now;
                break;
            case "handbook":
                profile.HandbookAcknowledgedAt = now;
                break;
        }

        await db.SaveChangesAsync(ct);
    }
}
