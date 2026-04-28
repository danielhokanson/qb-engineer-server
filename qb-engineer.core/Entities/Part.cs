using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Core.Entities;

public class Part : BaseAuditableEntity, IActiveAware
{
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Revision { get; set; } = "A";
    public PartStatus Status { get; set; } = PartStatus.Active;
    public PartType PartType { get; set; } = PartType.Part;
    public string? Material { get; set; }
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

    // IActiveAware — Phase 3 H2 active-check. Parts use a status enum rather
    // than a bool. "Active" / "Prototype" / "Draft" are usable on new
    // transactions; "Obsolete" is not (treated as deactivated).
    public bool IsActiveForNewTransactions => Status != PartStatus.Obsolete;
    public string GetDisplayName() => string.IsNullOrWhiteSpace(PartNumber)
        ? Description
        : $"{PartNumber} ({Description})";

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
