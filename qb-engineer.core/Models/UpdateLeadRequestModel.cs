using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdateLeadRequestModel(
    string? CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Source,
    LeadStatus? Status,
    string? Notes,
    DateTime? FollowUpDate,
    string? LostReason);
