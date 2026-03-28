namespace QBEngineer.Core.Entities;

public class PartPrice : BaseEntity
{
    public int PartId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }

    public Part Part { get; set; } = null!;
}
