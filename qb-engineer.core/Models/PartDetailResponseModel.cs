using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PartDetailResponseModel(
    int Id,
    string PartNumber,
    string Name,
    string? Description,
    string Revision,
    PartStatus Status,
    PartType PartType,
    // Pillar 1 — Decomposed type axes. Replaces the overloaded PartType
    // for new code; PartType stays on the wire two release cycles for rollback.
    ProcurementSource ProcurementSource,
    InventoryClass InventoryClass,
    int? ItemKindId,
    string? ItemKindLabel,
    // Tier 0 additions
    TraceabilityType TraceabilityType,
    AbcClass? AbcClass,
    string? ManufacturerName,
    string? ManufacturerPartNumber,
    string? Material,
    string? MoldToolRef,
    string? ExternalPartNumber,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    int? PreferredVendorId,
    string? PreferredVendorName,
    decimal? MinStockThreshold,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    int? SafetyStockDays,
    bool IsSerialTracked,
    int? ToolingAssetId,
    string? ToolingAssetName,
    // Workflow Pattern Phase 5 — surfaces cost gates so the workflow shell's
    // hasCost predicate can read the part's current cost state.
    decimal? ManualCostOverride,
    int? CurrentCostCalculationId,
    List<BOMEntryResponseModel> BomEntries,
    List<BOMUsageResponseModel> UsedIn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
