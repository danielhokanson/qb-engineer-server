namespace QBEngineer.Core.Entities;

public class ShipmentLine : BaseEntity
{
    public int ShipmentId { get; set; }
    public int SalesOrderLineId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public SalesOrderLine SalesOrderLine { get; set; } = null!;
}
