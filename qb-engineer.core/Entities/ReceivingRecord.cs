namespace QBEngineer.Core.Entities;

public class ReceivingRecord : BaseAuditableEntity
{
    public int PurchaseOrderLineId { get; set; }
    public int QuantityReceived { get; set; }
    public string? ReceivedBy { get; set; }
    public int? StorageLocationId { get; set; }
    public string? Notes { get; set; }

    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;
    public StorageLocation? StorageLocation { get; set; }
}
