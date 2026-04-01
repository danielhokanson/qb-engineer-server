using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Quote : BaseAuditableEntity
{
    public string QuoteNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? ShippingAddressId { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public DateTimeOffset? SentDate { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
    public DateTimeOffset? AcceptedDate { get; set; }
    public string? Notes { get; set; }
    public decimal TaxRate { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal TaxAmount => Subtotal * TaxRate;
    public decimal Total => Subtotal + TaxAmount;

    public Customer Customer { get; set; } = null!;
    public CustomerAddress? ShippingAddress { get; set; }
    public ICollection<QuoteLine> Lines { get; set; } = [];
    public SalesOrder? SalesOrder { get; set; }
}
