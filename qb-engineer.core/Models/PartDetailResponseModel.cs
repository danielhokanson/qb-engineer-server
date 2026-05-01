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
    // Pillar 2 — Tier 2: Material specification reference (FK to reference_data,
    // group_code = 'part.material_spec'). Replaces the free-text Material string.
    int? MaterialSpecId,
    string? MaterialSpecLabel,
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
    // Pillar 2 — Tier 2: measurement profile (canonical SI; *DisplayUnit columns
    // round-trip the user's typed unit).
    decimal? WeightEach,
    string? WeightDisplayUnit,
    decimal? LengthMm,
    decimal? WidthMm,
    decimal? HeightMm,
    string? DimensionDisplayUnit,
    decimal? VolumeMl,
    string? VolumeDisplayUnit,
    // Pillar 2 — Tier 2: valuation classification.
    int? ValuationClassId,
    string? ValuationClassLabel,
    // Pillar 2 — Tier 3: compliance + classification.
    string? HtsCode,
    string? HazmatClass,
    int? ShelfLifeDays,
    BackflushPolicy? BackflushPolicy,
    bool IsKit,
    bool IsConfigurable,
    int? DefaultBinId,
    int? SourcePartId,
    List<BOMEntryResponseModel> BomEntries,
    List<BOMUsageResponseModel> UsedIn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    // Pillar 4 Phase 2 surface — UoM cluster
    int? StockUomId = null,
    string? StockUomCode = null,
    string? StockUomLabel = null,
    int? PurchaseUomId = null,
    string? PurchaseUomCode = null,
    string? PurchaseUomLabel = null,
    int? SalesUomId = null,
    string? SalesUomCode = null,
    string? SalesUomLabel = null,
    // Pillar 4 Phase 2 surface — MRP cluster
    bool IsMrpPlanned = false,
    LotSizingRule? LotSizingRule = null,
    decimal? FixedOrderQuantity = null,
    decimal? MinimumOrderQuantity = null,
    decimal? OrderMultiple = null,
    int? PlanningFenceDays = null,
    int? DemandFenceDays = null,
    // Pillar 4 Phase 2 surface — Quality cluster
    bool RequiresReceivingInspection = false,
    int? ReceivingInspectionTemplateId = null,
    ReceivingInspectionFrequency? InspectionFrequency = null,
    int? InspectionSkipAfterN = null,
    // Pricing — resolved via IPartPricingResolver. EffectivePrice is non-nullable;
    // when no rung resolves, EffectivePriceSource is "Default" and EffectivePrice is 0.
    decimal EffectivePrice = 0m,
    string EffectivePriceCurrency = "USD",
    string EffectivePriceSource = "Default");
