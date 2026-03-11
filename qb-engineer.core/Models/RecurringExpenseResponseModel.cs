using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record RecurringExpenseResponseModel(
    int Id,
    int UserId,
    string UserName,
    decimal Amount,
    string Category,
    string Classification,
    string Description,
    string? Vendor,
    RecurrenceFrequency Frequency,
    DateTime NextOccurrenceDate,
    DateTime? LastGeneratedDate,
    DateTime? EndDate,
    bool IsActive,
    bool AutoApprove,
    DateTime CreatedAt);
