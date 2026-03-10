namespace QBEngineer.Core.Entities;

public class QuoteLine : BaseEntity
{
    public int QuoteId { get; set; }
    public int? PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int LineNumber { get; set; }
    public string? Notes { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public Quote Quote { get; set; } = null!;
    public Part? Part { get; set; }
}
