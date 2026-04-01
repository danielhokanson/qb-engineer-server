namespace QBEngineer.Core.Entities;

public class StatusEntry : BaseAuditableEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public string? Notes { get; set; }
    public int? SetById { get; set; }
}
