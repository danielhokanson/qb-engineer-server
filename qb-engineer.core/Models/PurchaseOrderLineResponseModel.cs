namespace QBEngineer.Core.Models;

// Phase 3 / WU-10 / F8-partial — quantities are decimal (was int).
public record PurchaseOrderLineResponseModel(
    int Id,
    int PartId,
    string PartNumber,
    string Description,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? Notes);
