using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class BinMovement : BaseEntity
{
    public string EntityType { get; set; } = "part";
    public int EntityId { get; set; }
    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public int MovedBy { get; set; }
    public DateTime MovedAt { get; set; }
    public BinMovementReason? Reason { get; set; }

    public StorageLocation? FromLocation { get; set; }
    public StorageLocation? ToLocation { get; set; }
}
