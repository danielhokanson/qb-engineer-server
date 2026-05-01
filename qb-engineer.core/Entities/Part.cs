using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Core.Entities;

public class Part : BaseAuditableEntity, IActiveAware
{
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Short human-readable identifier (e.g., "Sheath Mudkipper"). Required —
    /// the canonical "what is this thing" label rendered on cards, lists, and
    /// the detail-page heading. Backfilled from <see cref="Description"/> by
    /// migration <c>Add_Name_To_Part</c>.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Long-form notes (paragraph-length detail). Optional — fill in only when
    /// there is something meaningful beyond the short Name. Used to be
    /// double-duty as both name and description; that role moved to
    /// <see cref="Name"/>.
    /// </summary>
    public string? Description { get; set; }

    public string Revision { get; set; } = "A";
    public PartStatus Status { get; set; } = PartStatus.Active;

    /// <summary>
    /// Legacy overloaded type. Pillar 1 decomposed this into three orthogonal
    /// axes — see <see cref="ProcurementSource"/>, <see cref="InventoryClass"/>,
    /// and <see cref="ItemKindId"/>. Kept on the row for two release cycles
    /// for rollback safety; new code should prefer the three axes.
    /// </summary>
    public PartType PartType { get; set; } = PartType.Part;

    /// <summary>
    /// Pillar 1 — How the part is sourced (Make / Buy / Subcontract / Phantom).
    /// </summary>
    public ProcurementSource ProcurementSource { get; set; } = ProcurementSource.Buy;

    /// <summary>
    /// Pillar 1 — Which inventory bucket the part lives in (Raw / Component /
    /// Subassembly / FinishedGood / Consumable / Tool).
    /// </summary>
    public InventoryClass InventoryClass { get; set; } = InventoryClass.Component;

    /// <summary>
    /// Pillar 1 — Descriptive admin-configurable taxonomy (Fastener,
    /// Electronic, Packaging, Hardware, Material, etc.). FK to
    /// <c>reference_data</c> with group_code = 'part.item_kind'.
    /// </summary>
    public int? ItemKindId { get; set; }
    public ReferenceData? ItemKind { get; set; }

    /// <summary>
    /// Tier 0 — Replaces legacy <see cref="IsSerialTracked"/> boolean.
    /// Lot tracking is now expressible.
    /// </summary>
    public TraceabilityType TraceabilityType { get; set; } = TraceabilityType.None;

    /// <summary>
    /// Tier 0 — Cycle-counting frequency tier and stock-movement KPI bucket.
    /// </summary>
    public AbcClass? AbcClass { get; set; }

    /// <summary>
    /// Tier 0 — Primary manufacturer name (the engineering OEM, distinct from
    /// any distributor we buy through). Critical for COTS components.
    /// Distributor-side manufacturer-part-number lives on <c>VendorPart.VendorMpn</c>.
    /// </summary>
    public string? ManufacturerName { get; set; }

    /// <summary>
    /// Tier 0 — Manufacturer's part number (engineering identity). Customer
    /// drawings + datasheets reference this, not our internal SKU.
    /// </summary>
    public string? ManufacturerPartNumber { get; set; }

    /// <summary>
    /// Free-text material spec. Pillar 2 will replace with FK to
    /// <c>reference_data</c> (group_code = 'part.material_spec'). Kept as
    /// string for now to bound the Pillar 1+3 scope.
    /// </summary>
    public string? Material { get; set; }
    /// <summary>Pillar 1 — vestigial; superseded by <see cref="ToolingAssetId"/>. Keep for rollback.</summary>
    public string? MoldToolRef { get; set; }
    public string? ExternalPartNumber { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    // Preferred vendor & auto-PO
    public int? PreferredVendorId { get; set; }
    public int SafetyStockQty { get; set; }
    public bool ExcludeFromAutoPo { get; set; }
    public int? MinOrderQty { get; set; }
    public int? PackSize { get; set; }

    // Inventory thresholds & replenishment
    public decimal? MinStockThreshold { get; set; }
    public decimal? ReorderPoint { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public int? SafetyStockDays { get; set; }

    // MRP planning
    public LotSizingRule? LotSizingRule { get; set; }
    public decimal? FixedOrderQuantity { get; set; }
    public decimal? MinimumOrderQuantity { get; set; }
    public decimal? OrderMultiple { get; set; }
    public int? PlanningFenceDays { get; set; }
    public int? DemandFenceDays { get; set; }
    public bool IsMrpPlanned { get; set; }

    // Receiving inspection
    public bool RequiresReceivingInspection { get; set; }
    public int? ReceivingInspectionTemplateId { get; set; }
    public ReceivingInspectionFrequency InspectionFrequency { get; set; } = ReceivingInspectionFrequency.Every;
    public int? InspectionSkipAfterN { get; set; }

    // Serial tracking
    public bool IsSerialTracked { get; set; }

    // Custom fields (JSONB)
    public string? CustomFieldValues { get; set; }

    // Units of measure
    public int? StockUomId { get; set; }
    public int? PurchaseUomId { get; set; }
    public int? SalesUomId { get; set; }
    public UnitOfMeasure? StockUom { get; set; }
    public UnitOfMeasure? PurchaseUom { get; set; }
    public UnitOfMeasure? SalesUom { get; set; }

    // Tooling association
    public int? ToolingAssetId { get; set; }
    public Asset? ToolingAsset { get; set; }

    public Vendor? PreferredVendor { get; set; }

    // Workflow Pattern Phase 2 / D3 — Per-record cost override + pointer at the
    // active CostCalculation snapshot. Read priority for displayed cost:
    //   ManualCostOverride ?? CostCalculation.ResultAmount ?? null.
    // Until D5 ships, only ManualCostOverride is ever populated.
    public decimal? ManualCostOverride { get; set; }
    public int? CurrentCostCalculationId { get; set; }
    public CostCalculation? CurrentCostCalculation { get; set; }

    // IActiveAware — Phase 3 H2 active-check. Parts use a status enum rather
    // than a bool. "Active" / "Prototype" / "Draft" are usable on new
    // transactions; "Obsolete" is not (treated as deactivated).
    public bool IsActiveForNewTransactions => Status != PartStatus.Obsolete;
    public string GetDisplayName() => string.IsNullOrWhiteSpace(PartNumber)
        ? Name
        : $"{PartNumber} ({Name})";

    public ICollection<BOMEntry> BOMEntries { get; set; } = [];
    public ICollection<BOMEntry> UsedInBOM { get; set; } = [];
    public ICollection<Operation> Operations { get; set; } = [];
    public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = [];
    public ICollection<PartAlternate> Alternates { get; set; } = [];
    public ICollection<SerialNumber> SerialNumbers { get; set; } = [];

    // Phase 3 H4 / WU-20 — BOM revision history. CurrentBomRevisionId points
    // at the active immutable snapshot of this part's BOM. Older revisions
    // hang off the part via the BomRevisions collection but stay frozen.
    public int? CurrentBomRevisionId { get; set; }
    public BomRevision? CurrentBomRevision { get; set; }
    public ICollection<BomRevision> BomRevisions { get; set; } = [];
}
