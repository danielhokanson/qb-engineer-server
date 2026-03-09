namespace QBEngineer.Core.Models;

public record UpdateCustomerRequestModel(
    string? Name,
    string? CompanyName,
    string? Email,
    string? Phone,
    bool? IsActive);
