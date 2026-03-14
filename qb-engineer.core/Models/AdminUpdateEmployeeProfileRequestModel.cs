using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record AdminUpdateEmployeeProfileRequestModel(
    // Employment
    DateTime? StartDate,
    string? Department,
    string? JobTitle,
    string? EmployeeNumber,
    PayType? PayType,
    decimal? HourlyRate,
    decimal? SalaryAmount,

    // Tax/Compliance completion dates
    DateTime? W4CompletedAt,
    DateTime? StateWithholdingCompletedAt,
    DateTime? I9CompletedAt,
    DateTime? I9ExpirationDate,
    DateTime? DirectDepositCompletedAt,
    DateTime? WorkersCompAcknowledgedAt,
    DateTime? HandbookAcknowledgedAt);
