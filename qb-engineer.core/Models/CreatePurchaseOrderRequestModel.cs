namespace QBEngineer.Core.Models;

public record CreatePurchaseOrderRequestModel(
    int VendorId,
    int? JobId,
    string? Notes,
    List<CreatePurchaseOrderLineModel> Lines);

public record CreatePurchaseOrderLineModel(
    int PartId,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    string? Notes);
