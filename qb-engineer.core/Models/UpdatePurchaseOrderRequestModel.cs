namespace QBEngineer.Core.Models;

public record UpdatePurchaseOrderRequestModel(
    string? Notes,
    DateTimeOffset? ExpectedDeliveryDate);
