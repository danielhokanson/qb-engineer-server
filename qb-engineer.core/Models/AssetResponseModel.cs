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
    bool IsCustomerOwned,
    int? CavityCount,
    int? ToolLifeExpectancy,
    int CurrentShotCount,
    int? SourceJobId,
    string? SourceJobNumber,
    int? SourcePartId,
    string? SourcePartNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt);
