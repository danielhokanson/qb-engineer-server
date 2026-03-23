namespace QBEngineer.Core.Entities;

public class TrainingPathEnrollment : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int PathId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsAutoAssigned { get; set; } = false;
    public int? AssignedByUserId { get; set; }

    public TrainingPath Path { get; set; } = null!;
}
