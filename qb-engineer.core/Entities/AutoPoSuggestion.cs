using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class AutoPoSuggestion : BaseAuditableEntity
{
    public int PartId { get; set; }
    public int VendorId { get; set; }
    public int SuggestedQty { get; set; }
    public DateTimeOffset NeededByDate { get; set; }
    public string? SourceSalesOrderIds { get; set; }
    public AutoPoSuggestionStatus Status { get; set; } = AutoPoSuggestionStatus.Pending;
    public int? ConvertedPurchaseOrderId { get; set; }

    public Part Part { get; set; } = null!;
    public Vendor Vendor { get; set; } = null!;
    public PurchaseOrder? ConvertedPurchaseOrder { get; set; }
}
