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
        var query = db.ComplianceFormTemplates
            .AsNoTracking()
            .Include(t => t.FormDefinitionVersions);

        var filtered = request.IncludeInactive
            ? query
            : query.Where(t => t.IsActive);

        var templates = await filtered
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);

        return templates.Select(ComplianceTemplateMapper.ToResponse).ToList();
    }
}
