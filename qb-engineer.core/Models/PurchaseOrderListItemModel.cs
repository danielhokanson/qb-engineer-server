namespace QBEngineer.Core.Models;

// Phase 3 / WU-10 / F8-partial — TotalOrdered / TotalReceived are decimal
// (aggregate of decimal line quantities, was int).
public record PurchaseOrderListItemModel(
    int Id,
    string PONumber,
    int VendorId,
    string VendorName,
    int? JobId,
    string? JobNumber,
    string Status,
    int LineCount,
    decimal TotalOrdered,
    decimal TotalReceived,
    DateTimeOffset? ExpectedDeliveryDate,
    bool IsBlanket,
    DateTimeOffset CreatedAt);
