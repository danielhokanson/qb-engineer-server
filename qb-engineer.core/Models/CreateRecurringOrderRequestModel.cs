namespace QBEngineer.Core.Models;

public record CreateRecurringOrderRequestModel(
    string Name,
    int CustomerId,
    int? ShippingAddressId,
    int IntervalDays,
    DateTime NextGenerationDate,
    string? Notes,
    List<CreateRecurringOrderLineModel> Lines);

public record CreateRecurringOrderLineModel(
    int PartId,
    string Description,
    int Quantity,
    decimal UnitPrice);
