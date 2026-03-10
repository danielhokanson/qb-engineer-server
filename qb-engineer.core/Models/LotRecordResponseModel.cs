namespace QBEngineer.Core.Models;

public record LotRecordResponseModel(
    int Id,
    string LotNumber,
    int PartId,
    string PartNumber,
    string? PartDescription,
    int? JobId,
    string? JobNumber,
    int? ProductionRunId,
    int? PurchaseOrderLineId,
    int Quantity,
    DateTime? ExpirationDate,
    string? SupplierLotNumber,
    string? Notes,
    DateTime CreatedAt);
