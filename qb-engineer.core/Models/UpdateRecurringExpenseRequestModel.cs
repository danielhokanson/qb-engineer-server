using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdateRecurringExpenseRequestModel(
    decimal? Amount,
    string? Category,
    string? Classification,
    string? Description,
    string? Vendor,
    RecurrenceFrequency? Frequency,
    DateTime? EndDate,
    bool? IsActive,
    bool? AutoApprove);
