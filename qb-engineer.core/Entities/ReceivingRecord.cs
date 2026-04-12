using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class ReceivingRecord : BaseAuditableEntity
{
    public int PurchaseOrderLineId { get; set; }
    public int QuantityReceived { get; set; }
    public string? ReceivedBy { get; set; }
    public int? StorageLocationId { get; set; }
    public string? Notes { get; set; }
    public ReceivingInspectionStatus InspectionStatus { get; set; } = ReceivingInspectionStatus.NotRequired;
    public int? InspectedById { get; set; }
    public DateTimeOffset? InspectedAt { get; set; }
    public string? InspectionNotes { get; set; }
    public decimal? InspectedQuantityAccepted { get; set; }
    public decimal? InspectedQuantityRejected { get; set; }
    public int? QcInspectionId { get; set; }

    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;
    public StorageLocation? StorageLocation { get; set; }
}
