namespace QBEngineer.Core.Models;

public record ReceivingRecordResponseModel(
    int Id,
    int QuantityReceived,
    string? ReceivedBy,
    int? StorageLocationId,
    string? StorageLocationName,
    string? Notes,
    DateTime CreatedAt);
