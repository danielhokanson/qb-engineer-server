using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record DeleteComplianceTemplateCommand(int Id) : IRequest;

public class DeleteComplianceTemplateHandler(AppDbContext db)
    : IRequestHandler<DeleteComplianceTemplateCommand>
{
    private static readonly HashSet<ComplianceFormType> SystemFormTypes =
    [
        ComplianceFormType.W4,
        ComplianceFormType.I9,
        ComplianceFormType.StateWithholding,
        ComplianceFormType.DirectDeposit,
        ComplianceFormType.WorkersComp,
        ComplianceFormType.Handbook,
    ];

    public async Task Handle(DeleteComplianceTemplateCommand request, CancellationToken ct)
    {
        var template = await db.ComplianceFormTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Compliance template {request.Id} not found.");

        if (SystemFormTypes.Contains(template.FormType))
            throw new InvalidOperationException("System compliance templates cannot be deleted.");

        template.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
