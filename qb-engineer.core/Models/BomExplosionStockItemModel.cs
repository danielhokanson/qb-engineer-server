namespace QBEngineer.Core.Models;

public record BomExplosionStockItemModel(
    int PartId,
    string PartNumber,
    string Description,
    decimal Quantity);
