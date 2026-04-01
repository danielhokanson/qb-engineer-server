namespace QBEngineer.Core.Models;

public record ReceivingRecordResponseModel(
    int Id,
    int PurchaseOrderLineId,
    string? PurchaseOrderNumber,
    int? PartId,
    string? PartNumber,
    int QuantityReceived,
    string? ReceivedBy,
    int? StorageLocationId,
    string? StorageLocationName,
    string? LotNumber,
    string? Notes,
    DateTimeOffset CreatedAt);
