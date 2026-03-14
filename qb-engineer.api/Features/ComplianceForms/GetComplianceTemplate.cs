using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record GetComplianceTemplateQuery(int Id) : IRequest<ComplianceFormTemplateResponseModel>;

public class GetComplianceTemplateHandler(AppDbContext db)
    : IRequestHandler<GetComplianceTemplateQuery, ComplianceFormTemplateResponseModel>
{
    public async Task<ComplianceFormTemplateResponseModel> Handle(
        GetComplianceTemplateQuery request, CancellationToken ct)
    {
        var t = await db.ComplianceFormTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Compliance template {request.Id} not found.");

        return new ComplianceFormTemplateResponseModel(
            t.Id, t.Name, t.FormType, t.Description, t.Icon, t.SourceUrl,
            t.IsAutoSync, t.IsActive, t.SortOrder, t.RequiresIdentityDocs,
            t.DocuSealTemplateId, t.LastSyncedAt, t.ManualOverrideFileId,
            t.BlocksJobAssignment, t.ProfileCompletionKey,
            t.CreatedAt, t.UpdatedAt
        );
    }
}
