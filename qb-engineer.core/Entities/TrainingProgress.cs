using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class TrainingProgress : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public TrainingProgressStatus Status { get; set; }
    public int? QuizScore { get; set; }
    public int? QuizAttempts { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TimeSpentSeconds { get; set; }
    public string? QuizAnswersJson { get; set; }
    public int? WalkthroughStepReached { get; set; }

    public TrainingModule Module { get; set; } = null!;
}
