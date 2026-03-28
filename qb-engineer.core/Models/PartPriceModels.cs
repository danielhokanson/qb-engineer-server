namespace QBEngineer.Core.Models;

public record PartPriceResponseModel(
    int Id,
    int PartId,
    decimal UnitPrice,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string? Notes,
    bool IsCurrent);

public record AddPartPriceRequestModel(
    decimal UnitPrice,
    DateTime? EffectiveFrom,
    string? Notes);
