using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class BinContent : BaseEntity
{
    public int LocationId { get; set; }
    public string EntityType { get; set; } = "part"; // part, production_run, assembly, tooling
    public int EntityId { get; set; }
    public decimal Quantity { get; set; }
    public string? LotNumber { get; set; }
    public int? JobId { get; set; }
    public BinContentStatus Status { get; set; } = BinContentStatus.Stored;
    public int PlacedBy { get; set; }
    public DateTime PlacedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
    public int? RemovedBy { get; set; }
    public string? Notes { get; set; }

    public StorageLocation Location { get; set; } = null!;
    public Job? Job { get; set; }
}
