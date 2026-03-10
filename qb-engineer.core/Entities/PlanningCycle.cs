using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class PlanningCycle : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Goals { get; set; }
    public PlanningCycleStatus Status { get; set; } = PlanningCycleStatus.Planning;
    public int DurationDays { get; set; } = 14;

    public ICollection<PlanningCycleEntry> Entries { get; set; } = [];
}
