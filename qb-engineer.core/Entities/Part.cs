using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Part : BaseAuditableEntity
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

    // Preferred vendor
    public int? PreferredVendorId { get; set; }

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
    public ICollection<BOMEntry> BOMEntries { get; set; } = [];
    public ICollection<BOMEntry> UsedInBOM { get; set; } = [];
    public ICollection<Operation> Operations { get; set; } = [];
    public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = [];
    public ICollection<PartAlternate> Alternates { get; set; } = [];
}
