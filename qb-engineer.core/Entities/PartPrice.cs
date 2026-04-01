namespace QBEngineer.Core.Entities;

public class PartPrice : BaseEntity
{
    public int PartId { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string? Notes { get; set; }

    public Part Part { get; set; } = null!;
}
