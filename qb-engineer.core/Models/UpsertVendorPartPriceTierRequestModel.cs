namespace QBEngineer.Core.Models;

/// <summary>
/// Pillar 3 — Body for upserting a tiered-price row on a VendorPart. Upsert
/// key is (VendorPartId, MinQuantity, EffectiveFrom) — re-posting the same
/// triple updates the matched row's UnitPrice / Currency / EffectiveTo /
/// Notes; a unique triple inserts a new row.
/// </summary>
public record UpsertVendorPartPriceTierRequestModel(
    decimal MinQuantity,
    decimal UnitPrice,
    string Currency,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? Notes);
