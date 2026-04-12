using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record StartTimerRequestModel(
    int? JobId,
    string? Category,
    string? Notes,
    int? OperationId = null,
    TimeEntryType EntryType = TimeEntryType.Run);
