namespace QBEngineer.Core.Models;

public record CreatePriceListRequestModel(
    string Name,
    string? Description,
    int? CustomerId,
    bool IsDefault,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    List<CreatePriceListEntryModel> Entries);

public record CreatePriceListEntryModel(
    int PartId,
    decimal UnitPrice,
    int MinQuantity);
