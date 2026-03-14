using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ComplianceForms;

public record GetUserComplianceDetailQuery(int UserId) : IRequest<UserComplianceDetailResponseModel>;

public class GetUserComplianceDetailHandler(AppDbContext db)
    : IRequestHandler<GetUserComplianceDetailQuery, UserComplianceDetailResponseModel>
{
    public async Task<UserComplianceDetailResponseModel> Handle(
        GetUserComplianceDetailQuery request, CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var submissions = await db.ComplianceFormSubmissions
            .AsNoTracking()
            .Include(s => s.Template)
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        var identityDocs = await db.IdentityDocuments
            .AsNoTracking()
            .Include(d => d.FileAttachment)
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);

        var verifierIds = identityDocs
            .Where(d => d.VerifiedById.HasValue)
            .Select(d => d.VerifiedById!.Value)
            .Distinct()
            .ToList();

        var verifiers = verifierIds.Count > 0
            ? await db.Users.AsNoTracking()
                .Where(u => verifierIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.LastName}, {u.FirstName}", ct)
            : new Dictionary<int, string>();

        var submissionModels = submissions.Select(s => new ComplianceFormSubmissionResponseModel(
            s.Id, s.TemplateId, s.Template.Name, s.Template.FormType,
            s.Status, s.SignedAt, s.SignedPdfFileId, s.DocuSealSubmitUrl,
            s.CreatedAt
        )).ToList();

        var identityDocModels = identityDocs.Select(d => new IdentityDocumentResponseModel(
            d.Id, d.UserId, d.DocumentType, d.FileAttachmentId,
            d.FileAttachment.FileName, d.VerifiedAt, d.VerifiedById,
            d.VerifiedById.HasValue && verifiers.TryGetValue(d.VerifiedById.Value, out var name) ? name : null,
            d.ExpiresAt, d.Notes, d.CreatedAt
        )).ToList();

        return new UserComplianceDetailResponseModel(
            user.Id,
            $"{user.LastName}, {user.FirstName}",
            user.Email ?? string.Empty,
            submissionModels,
            identityDocModels
        );
    }
}
