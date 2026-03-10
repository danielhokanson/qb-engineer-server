namespace QBEngineer.Core.Models;

public record PurchaseOrderListItemModel(
    int Id,
    string PONumber,
    int VendorId,
    string VendorName,
    int? JobId,
    string? JobNumber,
    string Status,
    int LineCount,
    int TotalOrdered,
    int TotalReceived,
    DateTime? ExpectedDeliveryDate,
    DateTime CreatedAt);
