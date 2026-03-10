namespace QBEngineer.Core.Entities;

/// <summary>
/// ⚡ ACCOUNTING BOUNDARY — Standalone mode: full CRUD. Integrated mode: read-only cache.
/// </summary>
public class InvoiceLine : BaseEntity
{
    public int InvoiceId { get; set; }
    public int? PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int LineNumber { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public Invoice Invoice { get; set; } = null!;
    public Part? Part { get; set; }
}
