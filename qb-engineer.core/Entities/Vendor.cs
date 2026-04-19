using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Vendor : BaseAuditableEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public AutoPoMode? AutoPoMode { get; set; }
    public decimal? MinOrderAmount { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}
