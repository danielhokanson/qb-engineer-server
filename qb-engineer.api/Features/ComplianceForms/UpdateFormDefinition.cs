using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record UpdateFormDefinitionCommand(
    int TemplateId,
    string FormDefinitionJson,
    string? Revision) : IRequest<ComplianceFormTemplateResponseModel>;

public class UpdateFormDefinitionHandler(AppDbContext db)
    : IRequestHandler<UpdateFormDefinitionCommand, ComplianceFormTemplateResponseModel>
{
    public async Task<ComplianceFormTemplateResponseModel> Handle(
        UpdateFormDefinitionCommand request, CancellationToken ct)
    {
        var template = await db.ComplianceFormTemplates
            .Include(t => t.FormDefinitionVersions)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, ct)
            ?? throw new KeyNotFoundException($"Compliance template {request.TemplateId} not found.");

        var now = DateTime.UtcNow;
        var revision = request.Revision ?? now.ToString("yyyy-MM");
        var fieldCount = System.Text.RegularExpressions.Regex.Matches(request.FormDefinitionJson, @"""id""").Count;

        // Expire the current active version
        var currentVersion = template.FormDefinitionVersions
            .Where(v => v.IsActive && v.ExpirationDate == null)
            .OrderByDescending(v => v.EffectiveDate)
            .FirstOrDefault();

        if (currentVersion is not null)
            currentVersion.ExpirationDate = now;

        // Create new version from the admin-provided definition
        var version = new FormDefinitionVersion
        {
            TemplateId = template.Id,
            FormDefinitionJson = request.FormDefinitionJson,
            EffectiveDate = now,
            Revision = revision,
            ExtractedAt = now,
            FieldCount = fieldCount,
            IsActive = true,
        };
        db.FormDefinitionVersions.Add(version);
        await db.SaveChangesAsync(ct);

        // Reload versions for response mapping
        template.FormDefinitionVersions.Add(version);
        return ComplianceTemplateMapper.ToResponse(template);
    }
}
