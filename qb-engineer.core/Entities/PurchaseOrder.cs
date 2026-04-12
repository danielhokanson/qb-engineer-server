using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class PurchaseOrder : BaseAuditableEntity
{
    public string PONumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public int? JobId { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTimeOffset? SubmittedDate { get; set; }
    public DateTimeOffset? AcknowledgedDate { get; set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    public DateTimeOffset? ReceivedDate { get; set; }
    public string? Notes { get; set; }

    // Blanket PO fields
    public bool IsBlanket { get; set; }
    public decimal? BlanketTotalQuantity { get; set; }
    public decimal? BlanketReleasedQuantity { get; set; }
    public decimal? BlanketRemainingQuantity => BlanketTotalQuantity - BlanketReleasedQuantity;
    public DateTimeOffset? BlanketExpirationDate { get; set; }
    public decimal? AgreedUnitPrice { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    public Vendor Vendor { get; set; } = null!;
    public Job? Job { get; set; }
    public ICollection<PurchaseOrderLine> Lines { get; set; } = [];
    public ICollection<PurchaseOrderRelease> Releases { get; set; } = [];
}
