namespace QBEngineer.Core.Models;

public record ReceivePurchaseOrderRequestModel(
    int PurchaseOrderLineId,
    int QuantityReceived,
    int? LocationId,
    string? LotNumber,
    string? Notes);
