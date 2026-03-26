using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.ComplianceForms;

/// <summary>
/// Maps ComplianceFormTemplate → ComplianceFormTemplateResponseModel,
/// resolving the current FormDefinitionVersion (effective now, not expired).
/// Requires FormDefinitionVersions to be loaded/included on the template.
/// </summary>
public static class ComplianceTemplateMapper
{
    public static ComplianceFormTemplateResponseModel ToResponse(ComplianceFormTemplate t)
    {
        var now = DateTime.UtcNow;
        var currentVersion = t.FormDefinitionVersions?
            .Where(v => v.IsActive && v.EffectiveDate <= now && (v.ExpirationDate == null || v.ExpirationDate > now))
            .OrderByDescending(v => v.EffectiveDate)
            .FirstOrDefault();

        return new ComplianceFormTemplateResponseModel(
            t.Id, t.Name, t.FormType, t.Description, t.Icon, t.SourceUrl,
            t.IsAutoSync, t.IsActive, t.SortOrder, t.RequiresIdentityDocs,
            t.DocuSealTemplateId, t.LastSyncedAt, t.ManualOverrideFileId,
            t.BlocksJobAssignment, t.ProfileCompletionKey,
            currentVersion?.Id,
            currentVersion?.FormDefinitionJson,
            currentVersion?.Revision,
            t.CreatedAt, t.UpdatedAt,
            t.AcroFieldMapJson, t.FilledPdfTemplateId);
    }
}
