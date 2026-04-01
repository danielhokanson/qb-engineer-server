namespace QBEngineer.Core.Models;

public record PurchaseOrderDetailResponseModel(
    int Id,
    string PONumber,
    int VendorId,
    string VendorName,
    int? JobId,
    string? JobNumber,
    string Status,
    DateTimeOffset? SubmittedDate,
    DateTimeOffset? AcknowledgedDate,
    DateTimeOffset? ExpectedDeliveryDate,
    DateTimeOffset? ReceivedDate,
    string? Notes,
    List<PurchaseOrderLineResponseModel> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
