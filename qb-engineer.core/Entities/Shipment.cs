using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Shipment : BaseAuditableEntity
{
    public string ShipmentNumber { get; set; } = string.Empty;
    public int SalesOrderId { get; set; }
    public int? ShippingAddressId { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public decimal? ShippingCost { get; set; }
    public decimal? Weight { get; set; }
    public string? Notes { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
    public CustomerAddress? ShippingAddress { get; set; }
    public ICollection<ShipmentLine> Lines { get; set; } = [];
    public Invoice? Invoice { get; set; }
}
