using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateAssetRequestModel(
    string Name,
    AssetType AssetType,
    string? Location,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Notes);
