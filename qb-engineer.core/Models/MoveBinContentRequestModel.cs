using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record MoveBinContentRequestModel(
    int FromLocationId,
    int ToLocationId,
    decimal Quantity,
    BinMovementReason Reason);
