using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class TimeEntry : BaseAuditableEntity
{
    public int? JobId { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? TimerStart { get; set; }
    public DateTimeOffset? TimerStop { get; set; }
    public bool IsManual { get; set; }
    public bool IsLocked { get; set; }
    public string? AccountingTimeActivityId { get; set; }

    // Operation-level tracking
    public int? OperationId { get; set; }
    public TimeEntryType EntryType { get; set; } = TimeEntryType.Run;

    // Costing
    public decimal LaborCost { get; set; }
    public decimal BurdenCost { get; set; }

    // Navigation
    public Job? Job { get; set; }
    public Operation? Operation { get; set; }
}
