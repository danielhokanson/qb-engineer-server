using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class PpapElement : BaseEntity
{
    public int SubmissionId { get; set; }
    public int ElementNumber { get; set; }
    public string ElementName { get; set; } = string.Empty;
    public PpapElementStatus Status { get; set; } = PpapElementStatus.NotStarted;
    public bool IsRequired { get; set; }
    public string? Notes { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation (FK-only for ApplicationUser)
    public PpapSubmission Submission { get; set; } = null!;
}
