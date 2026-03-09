using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record BinStockResponseModel(
    int LocationId,
    string LocationName,
    string LocationPath,
    decimal Quantity,
    BinContentStatus Status,
    string? LotNumber);
