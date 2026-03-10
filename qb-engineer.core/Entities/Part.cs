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

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    // Preferred vendor
    public int? PreferredVendorId { get; set; }

    // Inventory thresholds
    public decimal? MinStockThreshold { get; set; }
    public decimal? ReorderPoint { get; set; }

    // Custom fields (JSONB)
    public string? CustomFieldValues { get; set; }

    public Vendor? PreferredVendor { get; set; }
    public ICollection<BOMEntry> BOMEntries { get; set; } = [];
    public ICollection<BOMEntry> UsedInBOM { get; set; } = [];
    public ICollection<PurchaseOrderLine> PurchaseOrderLines { get; set; } = [];
}
