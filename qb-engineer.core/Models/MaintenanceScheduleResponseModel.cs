namespace QBEngineer.Core.Models;

public record MaintenanceScheduleResponseModel(
    int Id,
    int AssetId,
    string AssetName,
    string Title,
    string? Description,
    int IntervalDays,
    decimal? IntervalHours,
    DateTime? LastPerformedAt,
    DateTime NextDueAt,
    bool IsActive,
    bool IsOverdue);
