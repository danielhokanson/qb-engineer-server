namespace QBEngineer.Core.Entities;

public class TimeEntry : BaseAuditableEntity
{
    public int? JobId { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public DateTime? TimerStart { get; set; }
    public DateTime? TimerStop { get; set; }
    public bool IsManual { get; set; }
    public bool IsLocked { get; set; }
    public string? AccountingTimeActivityId { get; set; }

    // Navigation
    public Job? Job { get; set; }
}
