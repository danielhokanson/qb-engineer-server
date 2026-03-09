using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record TimeEntryResponseModel(
    int Id,
    int? JobId,
    string? JobNumber,
    int UserId,
    string UserName,
    DateOnly Date,
    int DurationMinutes,
    string? Category,
    string? Notes,
    DateTime? TimerStart,
    DateTime? TimerStop,
    bool IsManual,
    bool IsLocked,
    DateTime CreatedAt);

public record CreateTimeEntryRequestModel(
    int? JobId,
    DateOnly Date,
    int DurationMinutes,
    string? Category,
    string? Notes);

public record StartTimerRequestModel(
    int? JobId,
    string? Category,
    string? Notes);

public record StopTimerRequestModel(
    string? Notes);

public record UpdateTimeEntryRequestModel(
    int? JobId,
    DateOnly? Date,
    int? DurationMinutes,
    string? Category,
    string? Notes);

public record ClockEventResponseModel(
    int Id,
    int UserId,
    string UserName,
    ClockEventType EventType,
    string? Reason,
    string? ScanMethod,
    DateTime Timestamp,
    string? Source);

public record CreateClockEventRequestModel(
    ClockEventType EventType,
    string? Reason,
    string? ScanMethod,
    string? Source);
