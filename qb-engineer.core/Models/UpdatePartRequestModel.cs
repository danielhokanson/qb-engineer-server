using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdatePartRequestModel(
    string? Name,
    string? Description,
    string? Revision,
    PartStatus? Status,
    PartType? PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalPartNumber,
    int? ToolingAssetId,
    int? PreferredVendorId,
    decimal? MinStockThreshold,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    int? SafetyStockDays,
    // Workflow Pattern Phase 5 — manual cost override (Tier 1 single-rate).
    // Sentinel value -1 means "clear to null".
    decimal? ManualCostOverride = null,
    // Pillar 1 / Tier 0 — manufacturer identity (engineering OEM, distinct
    // from the distributor-side MPN that lives on VendorPart).
    string? ManufacturerName = null,
    string? ManufacturerPartNumber = null,
    // Tier 0 — replaces legacy IsSerialTracked boolean.
    TraceabilityType? TraceabilityType = null,
    // Tier 0 — cycle-counting frequency tier. Empty string clears.
    AbcClass? AbcClass = null);
