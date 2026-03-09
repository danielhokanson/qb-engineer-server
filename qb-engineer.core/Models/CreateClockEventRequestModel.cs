using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateClockEventRequestModel(
    ClockEventType EventType,
    string? Reason,
    string? ScanMethod,
    string? Source);
