namespace QBEngineer.Core.Entities;

public class PurchaseOrderLine : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public int PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public int? MrpPlannedOrderId { get; set; }

    public int RemainingQuantity => OrderedQuantity - ReceivedQuantity;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Part Part { get; set; } = null!;
    public MrpPlannedOrder? MrpPlannedOrder { get; set; }
    public ICollection<ReceivingRecord> ReceivingRecords { get; set; } = [];
}
