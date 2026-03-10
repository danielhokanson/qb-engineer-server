namespace QBEngineer.Core.Models;

public record PurchaseOrderDetailResponseModel(
    int Id,
    string PONumber,
    int VendorId,
    string VendorName,
    int? JobId,
    string? JobNumber,
    string Status,
    DateTime? SubmittedDate,
    DateTime? AcknowledgedDate,
    DateTime? ExpectedDeliveryDate,
    DateTime? ReceivedDate,
    string? Notes,
    List<PurchaseOrderLineResponseModel> Lines,
    DateTime CreatedAt,
    DateTime UpdatedAt);
