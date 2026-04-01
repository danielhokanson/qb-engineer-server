using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class EmployeeProfile : BaseAuditableEntity
{
    public int UserId { get; set; }

    // Personal
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    // Address
    public string? Street1 { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    // Contact
    public string? PhoneNumber { get; set; }
    public string? PersonalEmail { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }

    // Employment (admin-editable)
    public DateTimeOffset? StartDate { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? EmployeeNumber { get; set; }
    public PayType? PayType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? SalaryAmount { get; set; }

    // Tax/Compliance (completion tracking — dates only, no actual tax data)
    public DateTimeOffset? W4CompletedAt { get; set; }
    public DateTimeOffset? StateWithholdingCompletedAt { get; set; }
    public DateTimeOffset? I9CompletedAt { get; set; }
    public DateTimeOffset? I9ExpirationDate { get; set; }
    public DateTimeOffset? DirectDepositCompletedAt { get; set; }
    public DateTimeOffset? WorkersCompAcknowledgedAt { get; set; }
    public DateTimeOffset? HandbookAcknowledgedAt { get; set; }

    // Set when user self-certifies onboarding complete without going through the wizard
    public DateTimeOffset? OnboardingBypassedAt { get; set; }
}

