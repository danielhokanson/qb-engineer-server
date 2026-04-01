namespace QBEngineer.Core.Entities;

public class DowntimeLog : BaseAuditableEntity
{
    public int AssetId { get; set; }
    public int? ReportedById { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public bool IsPlanned { get; set; }
    public string? Notes { get; set; }

    public decimal DurationHours => EndedAt.HasValue
        ? (decimal)(EndedAt.Value - StartedAt).TotalHours
        : (decimal)(DateTimeOffset.UtcNow - StartedAt).TotalHours;

    public Asset Asset { get; set; } = null!;
}
