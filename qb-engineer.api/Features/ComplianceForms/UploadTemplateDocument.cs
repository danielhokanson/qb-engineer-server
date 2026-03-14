using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record UploadTemplateDocumentCommand(int TemplateId, int FileAttachmentId) : IRequest<ComplianceFormTemplateResponseModel>;

public class UploadTemplateDocumentHandler(AppDbContext db)
    : IRequestHandler<UploadTemplateDocumentCommand, ComplianceFormTemplateResponseModel>
{
    public async Task<ComplianceFormTemplateResponseModel> Handle(
        UploadTemplateDocumentCommand request, CancellationToken ct)
    {
        var template = await db.ComplianceFormTemplates
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId, ct)
            ?? throw new KeyNotFoundException($"Compliance template {request.TemplateId} not found.");

        var file = await db.FileAttachments
            .FirstOrDefaultAsync(x => x.Id == request.FileAttachmentId, ct)
            ?? throw new KeyNotFoundException($"File attachment {request.FileAttachmentId} not found.");

        template.ManualOverrideFileId = request.FileAttachmentId;
        await db.SaveChangesAsync(ct);

        return new ComplianceFormTemplateResponseModel(
            template.Id, template.Name, template.FormType, template.Description, template.Icon,
            template.SourceUrl, template.IsAutoSync, template.IsActive, template.SortOrder,
            template.RequiresIdentityDocs, template.DocuSealTemplateId, template.LastSyncedAt,
            template.ManualOverrideFileId, template.BlocksJobAssignment, template.ProfileCompletionKey,
            template.CreatedAt, template.UpdatedAt);
    }
}
