using MediatR;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record CreateComplianceTemplateCommand(
    CreateComplianceFormTemplateRequestModel Model) : IRequest<ComplianceFormTemplateResponseModel>;

public class CreateComplianceTemplateHandler(AppDbContext db)
    : IRequestHandler<CreateComplianceTemplateCommand, ComplianceFormTemplateResponseModel>
{
    public async Task<ComplianceFormTemplateResponseModel> Handle(
        CreateComplianceTemplateCommand request, CancellationToken ct)
    {
        var m = request.Model;

        var template = new ComplianceFormTemplate
        {
            Name = m.Name,
            FormType = m.FormType,
            Description = m.Description,
            Icon = m.Icon,
            SourceUrl = m.SourceUrl,
            IsAutoSync = m.IsAutoSync,
            IsActive = m.IsActive,
            SortOrder = m.SortOrder,
            RequiresIdentityDocs = m.RequiresIdentityDocs,
            BlocksJobAssignment = m.BlocksJobAssignment,
            ProfileCompletionKey = m.ProfileCompletionKey,
        };

        db.ComplianceFormTemplates.Add(template);
        await db.SaveChangesAsync(ct);

        return new ComplianceFormTemplateResponseModel(
            template.Id, template.Name, template.FormType, template.Description,
            template.Icon, template.SourceUrl, template.IsAutoSync, template.IsActive,
            template.SortOrder, template.RequiresIdentityDocs, template.DocuSealTemplateId,
            template.LastSyncedAt, template.ManualOverrideFileId, template.BlocksJobAssignment,
            template.ProfileCompletionKey, template.CreatedAt, template.UpdatedAt
        );
    }
}
