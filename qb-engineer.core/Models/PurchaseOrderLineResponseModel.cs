namespace QBEngineer.Core.Models;

public record PurchaseOrderLineResponseModel(
    int Id,
    int PartId,
    string PartNumber,
    string Description,
    int OrderedQuantity,
    int ReceivedQuantity,
    int RemainingQuantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? Notes);
