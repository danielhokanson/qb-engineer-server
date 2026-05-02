using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PartListResponseModel(
    int Id,
    string PartNumber,
    string Name,
    string? Description,
    string Revision,
    PartStatus Status,
    // Pillar 1 — Decomposed type axes. The legacy single-axis PartType was
    // retired pre-beta; the list surface now exposes the three axes directly.
    ProcurementSource ProcurementSource,
    InventoryClass InventoryClass,
    int BomEntryCount,
    DateTimeOffset CreatedAt,
    // Pricing — resolved via IPartPricingResolver. EffectivePrice is non-nullable;
    // when no rung resolves, EffectivePriceSource is "Default" and EffectivePrice is 0.
    decimal EffectivePrice,
    string EffectivePriceCurrency,
    string EffectivePriceSource);
