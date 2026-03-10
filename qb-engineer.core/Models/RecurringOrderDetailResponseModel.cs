namespace QBEngineer.Core.Models;

public record RecurringOrderDetailResponseModel(
    int Id,
    string Name,
    int CustomerId,
    string CustomerName,
    int? ShippingAddressId,
    int IntervalDays,
    DateTime NextGenerationDate,
    DateTime? LastGeneratedDate,
    bool IsActive,
    string? Notes,
    List<RecurringOrderLineResponseModel> Lines,
    DateTime CreatedAt,
    DateTime UpdatedAt);
