namespace QBEngineer.Core.Entities;

public class SalesOrderLine : BaseEntity
{
    public int SalesOrderId { get; set; }
    public int? PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int LineNumber { get; set; }
    public int ShippedQuantity { get; set; }
    public string? Notes { get; set; }
    public int? UomId { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
    public int RemainingQuantity => Quantity - ShippedQuantity;
    public bool IsFullyShipped => ShippedQuantity >= Quantity;

    public SalesOrder SalesOrder { get; set; } = null!;
    public Part? Part { get; set; }
    public UnitOfMeasure? Uom { get; set; }
    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<ShipmentLine> ShipmentLines { get; set; } = [];
}
