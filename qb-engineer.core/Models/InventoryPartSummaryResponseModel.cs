namespace QBEngineer.Core.Models;

public record InventoryPartSummaryResponseModel(
    int PartId,
    string PartNumber,
    string Description,
    string? Material,
    decimal OnHand,
    decimal Reserved,
    decimal Available,
    List<BinStockResponseModel> BinLocations);
