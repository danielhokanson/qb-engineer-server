namespace QBEngineer.Core.Entities;

public class LotRecord : BaseAuditableEntity
{
    public string LotNumber { get; set; } = string.Empty;
    public int PartId { get; set; }
    public int? JobId { get; set; }
    public int? ProductionRunId { get; set; }
    public int? PurchaseOrderLineId { get; set; }
    public int Quantity { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? SupplierLotNumber { get; set; }
    public string? Notes { get; set; }

    public Part Part { get; set; } = null!;
    public Job? Job { get; set; }
    public ProductionRun? ProductionRun { get; set; }
    public PurchaseOrderLine? PurchaseOrderLine { get; set; }
}
