namespace QBEngineer.Core.Models;

public record CreateLeadRequestModel(
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Source,
    string? Notes,
    DateTime? FollowUpDate);
