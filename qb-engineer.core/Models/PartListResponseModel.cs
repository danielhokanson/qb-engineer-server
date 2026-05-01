using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PartListResponseModel(
    int Id,
    string PartNumber,
    string Name,
    string? Description,
    string Revision,
    PartStatus Status,
    PartType PartType,
    string? Material,
    string? ExternalPartNumber,
    int BomEntryCount,
    DateTimeOffset CreatedAt,
    // Pricing — resolved via IPartPricingResolver. EffectivePrice is non-nullable;
    // when no rung resolves, EffectivePriceSource is "Default" and EffectivePrice is 0.
    decimal EffectivePrice,
    string EffectivePriceCurrency,
    string EffectivePriceSource);
