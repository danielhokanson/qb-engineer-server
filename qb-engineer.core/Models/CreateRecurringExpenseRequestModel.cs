using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateRecurringExpenseRequestModel(
    decimal Amount,
    string Category,
    string Classification,
    string Description,
    string? Vendor,
    RecurrenceFrequency Frequency,
    DateTime StartDate,
    DateTime? EndDate,
    bool AutoApprove);
