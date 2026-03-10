namespace QBEngineer.Core.Models;

public record CreatePriceListRequestModel(
    string Name,
    string? Description,
    int? CustomerId,
    bool IsDefault,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    List<CreatePriceListEntryModel> Entries);

public record CreatePriceListEntryModel(
    int PartId,
    decimal UnitPrice,
    int MinQuantity);
