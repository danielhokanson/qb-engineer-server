namespace QBEngineer.Core.Models;

public record PartPriceResponseModel(
    int Id,
    int PartId,
    decimal UnitPrice,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? Notes,
    bool IsCurrent);

public record AddPartPriceRequestModel(
    decimal UnitPrice,
    DateTimeOffset? EffectiveFrom,
    string? Notes);
