using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record GetAllComplianceTemplatesQuery(bool IncludeInactive = false) : IRequest<List<ComplianceFormTemplateResponseModel>>;

public class GetAllComplianceTemplatesHandler(AppDbContext db)
    : IRequestHandler<GetAllComplianceTemplatesQuery, List<ComplianceFormTemplateResponseModel>>
{
    public async Task<List<ComplianceFormTemplateResponseModel>> Handle(
        GetAllComplianceTemplatesQuery request, CancellationToken ct)
    {
        var query = db.ComplianceFormTemplates.AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(t => t.IsActive);

        var templates = await query
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);

        return templates.Select(t => new ComplianceFormTemplateResponseModel(
            t.Id, t.Name, t.FormType, t.Description, t.Icon, t.SourceUrl,
            t.IsAutoSync, t.IsActive, t.SortOrder, t.RequiresIdentityDocs,
            t.DocuSealTemplateId, t.LastSyncedAt, t.ManualOverrideFileId,
            t.BlocksJobAssignment, t.ProfileCompletionKey,
            t.CreatedAt, t.UpdatedAt
        )).ToList();
    }
}
