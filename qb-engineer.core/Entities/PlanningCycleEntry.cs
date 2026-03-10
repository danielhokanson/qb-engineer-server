namespace QBEngineer.Core.Entities;

public class PlanningCycleEntry : BaseEntity
{
    public int PlanningCycleId { get; set; }
    public int JobId { get; set; }
    public DateTime CommittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsRolledOver { get; set; }
    public int SortOrder { get; set; }

    public PlanningCycle PlanningCycle { get; set; } = null!;
    public Job Job { get; set; } = null!;
}
