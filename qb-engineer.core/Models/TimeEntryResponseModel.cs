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
