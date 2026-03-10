namespace QBEngineer.Core.Models;

public record DowntimeLogResponseModel(
    int Id,
    int AssetId,
    string AssetName,
    int? ReportedById,
    DateTime StartedAt,
    DateTime? EndedAt,
    string Reason,
    string? Resolution,
    bool IsPlanned,
    string? Notes,
    decimal DurationHours,
    DateTime CreatedAt);
