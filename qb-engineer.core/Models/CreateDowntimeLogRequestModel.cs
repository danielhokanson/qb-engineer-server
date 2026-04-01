namespace QBEngineer.Core.Models;

public record CreateDowntimeLogRequestModel(
    int AssetId,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndedAt,
    string Reason,
    string? Resolution,
    bool IsPlanned,
    string? Notes);
