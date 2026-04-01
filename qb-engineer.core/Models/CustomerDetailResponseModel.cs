namespace QBEngineer.Core.Models;

public record CustomerDetailResponseModel(
    int Id,
    string Name,
    string? CompanyName,
    string? Email,
    string? Phone,
    bool IsActive,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    List<ContactResponseModel> Contacts,
    List<CustomerJobSummaryModel> Jobs);
