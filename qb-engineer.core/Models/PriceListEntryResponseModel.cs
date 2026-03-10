namespace QBEngineer.Core.Models;

public record PriceListEntryResponseModel(
    int Id,
    int PartId,
    string PartNumber,
    string PartDescription,
    decimal UnitPrice,
    int MinQuantity);
