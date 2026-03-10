namespace QBEngineer.Core.Entities;

public class RecurringOrderLine : BaseEntity
{
    public int RecurringOrderId { get; set; }
    public int PartId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int LineNumber { get; set; }

    public RecurringOrder RecurringOrder { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
