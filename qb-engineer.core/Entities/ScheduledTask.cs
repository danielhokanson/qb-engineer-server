namespace QBEngineer.Core.Entities;

public class ScheduledTask : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrackTypeId { get; set; }
    public int? InternalProjectTypeId { get; set; }
    public int? AssigneeId { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }

    public TrackType TrackType { get; set; } = null!;
    public ReferenceData? InternalProjectType { get; set; }
}
