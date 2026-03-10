namespace QBEngineer.Core.Models;

public record UpdatePurchaseOrderRequestModel(
    string? Notes,
    DateTime? ExpectedDeliveryDate);
