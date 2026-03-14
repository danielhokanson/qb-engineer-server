using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record HandleDocuSealWebhookCommand(
    int SubmissionId,
    string Status,
    DateTime? CompletedAt) : IRequest;

public class HandleDocuSealWebhookHandler(
    AppDbContext db,
    IDocumentSigningService signingService,
    IStorageService storageService,
    IOptions<MinioOptions> minioOptions,
    IMediator mediator)
    : IRequestHandler<HandleDocuSealWebhookCommand>
{
    public async Task Handle(HandleDocuSealWebhookCommand request, CancellationToken ct)
    {
        var submission = await db.ComplianceFormSubmissions
            .Include(s => s.Template)
            .FirstOrDefaultAsync(s => s.DocuSealSubmissionId == request.SubmissionId, ct)
            ?? throw new KeyNotFoundException(
                $"Submission with DocuSeal ID {request.SubmissionId} not found.");

        if (request.Status != "completed")
        {
            submission.Status = request.Status switch
            {
                "opened" => ComplianceSubmissionStatus.Opened,
                "expired" => ComplianceSubmissionStatus.Expired,
                "declined" => ComplianceSubmissionStatus.Declined,
                _ => submission.Status,
            };
            await db.SaveChangesAsync(ct);
            return;
        }

        // Download signed PDF from DocuSeal
        var signedPdfBytes = await signingService.GetSignedPdfAsync(request.SubmissionId, ct);

        // Upload to MinIO (PII bucket)
        var opts = minioOptions.Value;
        var objectKey = $"compliance/{submission.UserId}/{submission.TemplateId}/{Guid.NewGuid():N}-signed.pdf";

        using var stream = new MemoryStream(signedPdfBytes);
        await storageService.UploadAsync(opts.PiiDocsBucket, objectKey, stream, "application/pdf", ct);

        // Create FileAttachment record
        var fileAttachment = new FileAttachment
        {
            FileName = $"{submission.Template.Name}-signed.pdf",
            ContentType = "application/pdf",
            Size = signedPdfBytes.Length,
            BucketName = opts.PiiDocsBucket,
            ObjectKey = objectKey,
            EntityType = "compliance_submissions",
            EntityId = submission.Id,
            UploadedById = submission.UserId,
            Sensitivity = "pii",
        };

        db.Set<FileAttachment>().Add(fileAttachment);
        await db.SaveChangesAsync(ct);

        // Update submission
        submission.Status = ComplianceSubmissionStatus.Completed;
        submission.SignedAt = request.CompletedAt ?? DateTime.UtcNow;
        submission.SignedPdfFileId = fileAttachment.Id;

        await db.SaveChangesAsync(ct);

        // Acknowledge form completion on EmployeeProfile
        if (!string.IsNullOrEmpty(submission.Template.ProfileCompletionKey))
        {
            await mediator.Send(
                new EmployeeProfile.AcknowledgeFormCommand(
                    submission.UserId, submission.Template.ProfileCompletionKey),
                ct);
        }
    }
}
