namespace QBEngineer.Core.Entities;

public class Customer : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

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
