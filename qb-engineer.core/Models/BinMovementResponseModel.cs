using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

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
