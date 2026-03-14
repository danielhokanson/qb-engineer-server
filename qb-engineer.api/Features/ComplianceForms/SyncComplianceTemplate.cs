using System.Security.Cryptography;

using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record SyncComplianceTemplateCommand(int Id) : IRequest;

public class SyncComplianceTemplateHandler(
    AppDbContext db,
    IDocumentSigningService signingService,
    IHttpClientFactory httpClientFactory)
    : IRequestHandler<SyncComplianceTemplateCommand>
{
    public async Task Handle(SyncComplianceTemplateCommand request, CancellationToken ct)
    {
        var template = await db.ComplianceFormTemplates
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Compliance template {request.Id} not found.");

        if (string.IsNullOrEmpty(template.SourceUrl))
            throw new InvalidOperationException($"Template {request.Id} has no SourceUrl configured.");

        using var httpClient = httpClientFactory.CreateClient();
        var pdfBytes = await httpClient.GetByteArrayAsync(template.SourceUrl, ct);

        var newHash = Convert.ToHexStringLower(SHA256.HashData(pdfBytes));

        if (newHash == template.Sha256Hash)
        {
            template.LastSyncedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return;
        }

        var docuSealTemplateId = await signingService.CreateTemplateFromPdfAsync(
            template.Name, pdfBytes, ct);

        if (template.DocuSealTemplateId.HasValue)
        {
            try
            {
                await signingService.DeleteTemplateAsync(template.DocuSealTemplateId.Value, ct);
            }
            catch
            {
                // Old template cleanup is best-effort
            }
        }

        template.DocuSealTemplateId = docuSealTemplateId;
        template.Sha256Hash = newHash;
        template.LastSyncedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
