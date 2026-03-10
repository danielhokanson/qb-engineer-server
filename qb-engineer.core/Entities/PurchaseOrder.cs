using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class PurchaseOrder : BaseAuditableEntity
{
    public string PONumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public int? JobId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime? SubmittedDate { get; set; }
    public DateTime? AcknowledgedDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    public Vendor Vendor { get; set; } = null!;
    public Job? Job { get; set; }
    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
}
