namespace QBEngineer.Core.Models;

public record CreateLotRecordRequestModel(
    string? LotNumber,
    int PartId,
    int? JobId,
    int? ProductionRunId,
    int? PurchaseOrderLineId,
    int Quantity,
    DateTime? ExpirationDate,
    string? SupplierLotNumber,
    string? Notes);
