namespace QBEngineer.Core.Models;

public record VendorListItemModel(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    bool IsActive,
    int PoCount,
    DateTime CreatedAt);
