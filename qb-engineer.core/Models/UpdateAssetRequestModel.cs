using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

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
