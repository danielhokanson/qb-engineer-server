using System.Text.Json;

using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

using EmployeeProfileEntity = QBEngineer.Core.Entities.EmployeeProfile;

namespace QBEngineer.Api.Features.Onboarding;

/// <summary>
/// Persists profile data, identity documents, and acknowledgments WITHOUT
/// touching DocuSeal or filling any PDFs.
///
/// This is step 1 of the per-form review flow:
///   save → [preview-pdf → sign-form] × N forms → done
///
/// Returns the list of compliance form types that need to be signed so the
/// frontend can drive the per-form review loop.
/// </summary>
public record SaveOnboardingDataCommand(
    int UserId,
    string UserEmail,
    string UserName,
    OnboardingSubmitRequestModel Model) : IRequest<SaveOnboardingResultModel>;

public class SaveOnboardingDataHandler(AppDbContext db)
    : IRequestHandler<SaveOnboardingDataCommand, SaveOnboardingResultModel>
{
    public async Task<SaveOnboardingResultModel> Handle(
        SaveOnboardingDataCommand request, CancellationToken ct)
    {
        var m = request.Model;

        // ── 1. Persist profile ──────────────────────────────────────────────
        await UpsertProfileAsync(request.UserId, m, ct);

        // ── 2. Link identity documents ──────────────────────────────────────
        await SaveIdentityDocumentsAsync(request.UserId, m, ct);

        // ── 3. Mark acknowledgments ─────────────────────────────────────────
        if (m.AcknowledgeWorkersComp || m.AcknowledgeHandbook)
            await MarkAcknowledgmentsAsync(request.UserId, m.AcknowledgeWorkersComp, m.AcknowledgeHandbook, ct);

        // ── 4. Determine which forms need to be signed ──────────────────────
        var templates = await db.ComplianceFormTemplates
            .AsNoTracking()
            .Where(t => t.IsActive && new[]
            {
                ComplianceFormType.W4,
                ComplianceFormType.I9,
                ComplianceFormType.StateWithholding,
            }.Contains(t.FormType))
            .OrderBy(t => t.SortOrder)
            .ToListAsync(ct);

        var formsToSign = templates
            .Select(t => new OnboardingFormToSignItem(
                t.FormType.ToString(),
                t.Name,
                HasTemplate: !string.IsNullOrWhiteSpace(t.AcroFieldMapJson) && t.FilledPdfTemplateId is not null))
            .ToList();

        return new SaveOnboardingResultModel(formsToSign);
    }

    private async Task UpsertProfileAsync(int userId, OnboardingSubmitRequestModel m, CancellationToken ct)
    {
        var profile = await db.EmployeeProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new EmployeeProfileEntity { UserId = userId };
            db.EmployeeProfiles.Add(profile);
        }

        profile.PhoneNumber = m.Phone;
        profile.PersonalEmail = m.Email;
        profile.DateOfBirth = m.DateOfBirth;
        profile.Street1 = m.Street1;
        profile.Street2 = m.Street2;
        profile.City = m.City;
        profile.State = m.AddressState;
        profile.ZipCode = m.ZipCode;
        profile.Country = "US";

        if (!string.IsNullOrWhiteSpace(m.BankName) && !string.IsNullOrWhiteSpace(m.RoutingNumber))
            profile.DirectDepositCompletedAt ??= DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is not null)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName)) user.FirstName = m.FirstName;
            if (string.IsNullOrWhiteSpace(user.LastName)) user.LastName = m.LastName;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task SaveIdentityDocumentsAsync(int userId, OnboardingSubmitRequestModel m, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(m.I9DocumentChoice)) return;

        var docs = new List<(IdentityDocumentType DocType, int? AttachmentId, DateTime? ExpiresAt)>();

        if (m.I9DocumentChoice == "A" && m.I9ListAFileAttachmentId.HasValue)
        {
            docs.Add((IdentityDocumentType.ListA, m.I9ListAFileAttachmentId, m.I9ListAExpiry));
        }
        else if (m.I9DocumentChoice == "BC")
        {
            if (m.I9ListBFileAttachmentId.HasValue)
                docs.Add((IdentityDocumentType.ListB, m.I9ListBFileAttachmentId, m.I9ListBExpiry));
            if (m.I9ListCFileAttachmentId.HasValue)
                docs.Add((IdentityDocumentType.ListC, m.I9ListCFileAttachmentId, m.I9ListCExpiry));
        }

        foreach (var (docType, attachmentId, expiresAt) in docs)
        {
            if (attachmentId is null) continue;

            // Skip if already recorded for this user+type
            var existing = await db.IdentityDocuments
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DocumentType == docType, ct);
            if (existing is not null) continue;

            var identityDoc = new IdentityDocument
            {
                UserId = userId,
                DocumentType = docType,
                FileAttachmentId = attachmentId.Value,
                ExpiresAt = expiresAt.HasValue ? DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Utc) : null,
            };
            db.IdentityDocuments.Add(identityDoc);
            await db.SaveChangesAsync(ct);

            var attachment = await db.FileAttachments.FindAsync([attachmentId.Value], ct);
            if (attachment is not null)
            {
                attachment.EntityId = identityDoc.Id;
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private async Task MarkAcknowledgmentsAsync(
        int userId, bool workersComp, bool handbook, CancellationToken ct)
    {
        var profile = await db.EmployeeProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? new EmployeeProfileEntity { UserId = userId };

        var now = DateTime.UtcNow;
        if (workersComp) profile.WorkersCompAcknowledgedAt = now;
        if (handbook)    profile.HandbookAcknowledgedAt    = now;

        await db.SaveChangesAsync(ct);
    }
}
