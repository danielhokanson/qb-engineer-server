using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateStorageLocationRequestModel(
    string Name,
    LocationType LocationType,
    int? ParentId,
    string? Barcode,
    string? Description);
