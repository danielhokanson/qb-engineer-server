namespace QBEngineer.Core.Models;

public record AccountingSyncStatus(
    bool Connected,
    DateTime? LastSyncAt,
    int QueueDepth,
    int FailedCount);
