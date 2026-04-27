namespace QBEngineer.Core.Entities;

public class PurchaseOrderLine : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public int PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    // Phase 3 / WU-10 / F8-partial: quantities are decimal, not int. UoM-aware
    // shops need fractional quantities — material-by-weight (lb, kg), by-time
    // (hr), by-volume (gal, l). decimal(18, 4) = 4 fractional places, plenty
    // for any reasonable UoM. Cases EDGE-DECIMAL-PRECISION-001 / -004.
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public int? MrpPlannedOrderId { get; set; }
    public int? UomId { get; set; }

    public decimal RemainingQuantity => OrderedQuantity - ReceivedQuantity;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Part Part { get; set; } = null!;
    public MrpPlannedOrder? MrpPlannedOrder { get; set; }
    public UnitOfMeasure? Uom { get; set; }
    public ICollection<ReceivingRecord> ReceivingRecords { get; set; } = [];
    public ICollection<PurchaseOrderRelease> Releases { get; set; } = [];
}
