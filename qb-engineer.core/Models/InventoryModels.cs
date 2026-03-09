using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record StorageLocationResponseModel(
    int Id,
    string Name,
    LocationType LocationType,
    int? ParentId,
    string? Barcode,
    string? Description,
    int SortOrder,
    bool IsActive,
    string LocationPath,
    int ContentCount,
    List<StorageLocationResponseModel> Children);

public record StorageLocationFlatResponseModel(
    int Id,
    string Name,
    LocationType LocationType,
    string? Barcode,
    string LocationPath);

public record BinContentResponseModel(
    int Id,
    int LocationId,
    string LocationName,
    string LocationPath,
    string EntityType,
    int EntityId,
    string EntityName,
    decimal Quantity,
    string? LotNumber,
    int? JobId,
    string? JobNumber,
    BinContentStatus Status,
    DateTime PlacedAt);

public record BinMovementResponseModel(
    int Id,
    string EntityType,
    int EntityId,
    string EntityName,
    decimal Quantity,
    string? LotNumber,
    int? FromLocationId,
    string? FromLocationName,
    int? ToLocationId,
    string? ToLocationName,
    string MovedByName,
    DateTime MovedAt,
    BinMovementReason? Reason);

public record InventoryPartSummaryResponseModel(
    int PartId,
    string PartNumber,
    string Description,
    string? Material,
    decimal OnHand,
    decimal Reserved,
    decimal Available,
    List<BinStockResponseModel> BinLocations);

public record BinStockResponseModel(
    int LocationId,
    string LocationName,
    string LocationPath,
    decimal Quantity,
    BinContentStatus Status,
    string? LotNumber);

public record CreateStorageLocationRequestModel(
    string Name,
    LocationType LocationType,
    int? ParentId,
    string? Barcode,
    string? Description);

public record PlaceBinContentRequestModel(
    int LocationId,
    string EntityType,
    int EntityId,
    decimal Quantity,
    string? LotNumber,
    int? JobId,
    BinContentStatus Status,
    string? Notes);

public record MoveBinContentRequestModel(
    int FromLocationId,
    int ToLocationId,
    decimal Quantity,
    BinMovementReason Reason);
