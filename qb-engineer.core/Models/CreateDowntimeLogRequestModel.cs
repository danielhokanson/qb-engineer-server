namespace QBEngineer.Core.Models;

public record CreateDowntimeLogRequestModel(
    int AssetId,
    DateTime StartedAt,
    DateTime? EndedAt,
    string Reason,
    string? Resolution,
    bool IsPlanned,
    string? Notes);
