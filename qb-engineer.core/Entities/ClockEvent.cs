using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class ClockEvent : BaseEntity
{
    public int UserId { get; set; }
    public ClockEventType EventType { get; set; }
    public string? Reason { get; set; }
    public string? ScanMethod { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? Source { get; set; }
}
