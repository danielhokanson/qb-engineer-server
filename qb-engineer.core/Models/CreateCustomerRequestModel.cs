namespace QBEngineer.Core.Models;

public record CreateCustomerRequestModel(
    string Name,
    string? CompanyName,
    string? Email,
    string? Phone);
