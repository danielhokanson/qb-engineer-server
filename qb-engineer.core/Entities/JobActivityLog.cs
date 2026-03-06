using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class JobActivityLog : BaseEntity
{
    public int JobId { get; set; }
    public int? UserId { get; set; }
    public ActivityAction Action { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;
}
