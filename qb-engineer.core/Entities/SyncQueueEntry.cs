using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class SyncQueueEntry : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}
