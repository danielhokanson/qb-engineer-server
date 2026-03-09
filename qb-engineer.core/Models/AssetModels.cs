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

public record CreateAssetRequestModel(
    string Name,
    AssetType AssetType,
    string? Location,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Notes);

public record UpdateAssetRequestModel(
    string? Name,
    AssetType? AssetType,
    string? Location,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    AssetStatus? Status,
    decimal? CurrentHours,
    string? Notes);
