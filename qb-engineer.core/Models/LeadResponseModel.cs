using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record LeadResponseModel(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Source,
    LeadStatus Status,
    string? Notes,
    DateTime? FollowUpDate,
    string? LostReason,
    int? ConvertedCustomerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
