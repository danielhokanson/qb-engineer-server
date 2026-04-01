using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ClockEventResponseModel(
    int Id,
    int UserId,
    string UserName,
    ClockEventType EventType,
    string? Reason,
    string? ScanMethod,
    DateTimeOffset Timestamp,
    string? Source);
