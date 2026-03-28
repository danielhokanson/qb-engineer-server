using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class EmployeeProfile : BaseAuditableEntity
{
    public int UserId { get; set; }

    // Personal
    public DateTime? DateOfBirth { get; set; }
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
    public DateTime? StartDate { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? EmployeeNumber { get; set; }
    public PayType? PayType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? SalaryAmount { get; set; }

    // Tax/Compliance (completion tracking — dates only, no actual tax data)
    public DateTime? W4CompletedAt { get; set; }
    public DateTime? StateWithholdingCompletedAt { get; set; }
    public DateTime? I9CompletedAt { get; set; }
    public DateTime? I9ExpirationDate { get; set; }
    public DateTime? DirectDepositCompletedAt { get; set; }
    public DateTime? WorkersCompAcknowledgedAt { get; set; }
    public DateTime? HandbookAcknowledgedAt { get; set; }

    // Set when user self-certifies onboarding complete without going through the wizard
    public DateTime? OnboardingBypassedAt { get; set; }
}

