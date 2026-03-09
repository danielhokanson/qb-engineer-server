using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record AssetResponseModel(
    int Id,
    string Name,
    AssetType AssetType,
    string? Location,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    AssetStatus Status,
    string? PhotoFileId,
    decimal CurrentHours,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
