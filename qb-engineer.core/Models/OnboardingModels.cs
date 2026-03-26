namespace QBEngineer.Core.Models;

/// <summary>
/// Unified request model for the new-hire onboarding wizard.
/// Shared fields (personal info, address) are collected once and used for ALL government forms.
/// Form-specific fields are grouped by step so the correct instructions can be displayed.
/// The backend maps all fields to AcroForm fields via each template's AcroFieldMapJson.
/// </summary>
public record OnboardingSubmitRequestModel(
    // ── Step 1: Personal Information (shared across all forms) ──────────
    string FirstName,
    string? MiddleName,
    string LastName,
    string? OtherLastNames,
    DateTime DateOfBirth,
    string Ssn,
    string Email,
    string Phone,

    // ── Step 2: Home Address (shared across all forms) ───────────────────
    string Street1,
    string? Street2,
    string City,
    string AddressState,
    string ZipCode,

    // ── Step 3: W-4 Federal Withholding (form-specific) ─────────────────
    string W4FilingStatus,
    bool W4MultipleJobs,
    decimal W4ClaimDependentsAmount,
    decimal W4OtherIncome,
    decimal W4Deductions,
    decimal W4ExtraWithholding,
    bool W4ExemptFromWithholding,

    // ── Step 4: State Withholding (form-specific) ────────────────────────
    string? StateFilingStatus,
    int? StateAllowances,
    decimal? StateAdditionalWithholding,
    bool? StateExempt,

    // ── Step 5: I-9 Employment Eligibility (form-specific) ───────────────
    /// <summary>1=US Citizen, 2=Noncitizen National, 3=LPR, 4=Alien Authorized to Work</summary>
    string I9CitizenshipStatus,
    string? I9AlienRegNumber,
    string? I9I94Number,
    string? I9ForeignPassportNumber,
    string? I9ForeignPassportCountry,
    DateTime? I9WorkAuthExpiry,
    bool I9PreparedByPreparer,
    string? I9PreparerFirstName,
    string? I9PreparerLastName,
    string? I9PreparerAddress,
    string? I9PreparerCity,
    string? I9PreparerState,
    string? I9PreparerZip,

    // ── I-9 Identity Documents ───────────────────────────────────────────
    /// <summary>"A" = presented a List A document; "BC" = presented List B + List C documents.</summary>
    string? I9DocumentChoice,
    string? I9ListAType,
    string? I9ListADocNumber,
    string? I9ListAAuthority,
    DateTime? I9ListAExpiry,
    int? I9ListAFileAttachmentId,
    string? I9ListBType,
    string? I9ListBDocNumber,
    string? I9ListBAuthority,
    DateTime? I9ListBExpiry,
    int? I9ListBFileAttachmentId,
    string? I9ListCType,
    string? I9ListCDocNumber,
    string? I9ListCAuthority,
    DateTime? I9ListCExpiry,
    int? I9ListCFileAttachmentId,

    // ── Step 6: Direct Deposit ───────────────────────────────────────────
    string BankName,
    string RoutingNumber,
    string AccountNumber,
    string AccountType,

    // ── Step 7: Acknowledgments ─────────────────────────────────────────
    bool AcknowledgeWorkersComp,
    bool AcknowledgeHandbook
);

public record OnboardingSigningUrlModel(
    string FormType,
    string FormName,
    string SigningUrl,
    int SubmissionId);

public record OnboardingSubmitResultModel(
    bool RequiresSigning,
    IReadOnlyList<OnboardingSigningUrlModel> SigningUrls,
    int? I9EmployerDocuSealSubmitterId);

public record OnboardingStatusModel(
    bool W4Complete,
    bool I9Complete,
    bool StateWithholdingComplete,
    bool DirectDepositComplete,
    bool WorkersCompComplete,
    bool HandbookComplete,
    bool AllComplete,
    bool CanBeAssigned);
