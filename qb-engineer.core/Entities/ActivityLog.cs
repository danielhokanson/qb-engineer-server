namespace QBEngineer.Core.Entities;

public class ActivityLog : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
