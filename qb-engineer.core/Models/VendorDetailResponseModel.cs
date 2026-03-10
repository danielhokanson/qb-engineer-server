namespace QBEngineer.Core.Models;

public record VendorDetailResponseModel(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? PaymentTerms,
    string? Notes,
    bool IsActive,
    string? ExternalId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<PurchaseOrderListItemModel> PurchaseOrders);
