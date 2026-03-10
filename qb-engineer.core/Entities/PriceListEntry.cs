namespace QBEngineer.Core.Entities;

public class PriceListEntry : BaseEntity
{
    public int PriceListId { get; set; }
    public int PartId { get; set; }
    public decimal UnitPrice { get; set; }
    public int MinQuantity { get; set; } = 1;

    public PriceList PriceList { get; set; } = null!;
    public Part Part { get; set; } = null!;
}
