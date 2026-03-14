using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record EmployeeProfileResponseModel(
    int Id,
    int UserId,

    // Personal
    DateTime? DateOfBirth,
    string? Gender,

    // Address
    string? Street1,
    string? Street2,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,

    // Contact
    string? PhoneNumber,
    string? PersonalEmail,

    // Emergency
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship,

    // Employment
    DateTime? StartDate,
    string? Department,
    string? JobTitle,
    string? EmployeeNumber,
    PayType? PayType,
    decimal? HourlyRate,
    decimal? SalaryAmount,

    // Tax/Compliance
    DateTime? W4CompletedAt,
    DateTime? StateWithholdingCompletedAt,
    DateTime? I9CompletedAt,
    DateTime? I9ExpirationDate,
    DateTime? DirectDepositCompletedAt,
    DateTime? WorkersCompAcknowledgedAt,
    DateTime? HandbookAcknowledgedAt);
