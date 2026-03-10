namespace QBEngineer.Core.Models;

public record RecurringOrderListItemModel(
    int Id,
    string Name,
    int CustomerId,
    string CustomerName,
    int IntervalDays,
    DateTime NextGenerationDate,
    DateTime? LastGeneratedDate,
    bool IsActive,
    int LineCount,
    DateTime CreatedAt);
