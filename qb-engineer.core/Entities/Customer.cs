namespace QBEngineer.Core.Entities;

public class Customer : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    // Credit management
    public decimal? CreditLimit { get; set; }
    public bool IsOnCreditHold { get; set; }
    public string? CreditHoldReason { get; set; }
    public DateTimeOffset? CreditHoldAt { get; set; }
    public int? CreditHoldById { get; set; }
    public DateTimeOffset? LastCreditReviewDate { get; set; }
    public int? CreditReviewFrequencyDays { get; set; }

    // Tax handling — many B2B manufacturing customers are sales-tax-exempt
    // (resellers, government, non-profits). When IsTaxExempt is true, invoice
    // generation must skip the sales-tax line. The exemption ID is the
    // certificate number kept on file for audit purposes.
    public bool IsTaxExempt { get; set; }
    public string? TaxExemptionId { get; set; }

    // Default tax + currency used when invoicing this customer if the line/header
    // does not specify otherwise. Both nullable so the tenant default applies.
    // Phase 3 F3 — captured at create-time to avoid a PATCH-after-create round trip.
    public int? DefaultTaxCodeId { get; set; }
    public SalesTaxRate? DefaultTaxCode { get; set; }
    /// <summary>ISO 4217 3-letter currency code (e.g. "USD"). Null = use tenant default.</summary>
    public string? DefaultCurrency { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Job> Jobs { get; set; } = [];
    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<SalesOrder> SalesOrders { get; set; } = [];
    public ICollection<Quote> Quotes { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<PriceList> PriceLists { get; set; } = [];
    public ICollection<RecurringOrder> RecurringOrders { get; set; } = [];
}
