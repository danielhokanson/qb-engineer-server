using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class JobLink : BaseEntity
{
    public int SourceJobId { get; set; }
    public int TargetJobId { get; set; }
    public JobLinkType LinkType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job SourceJob { get; set; } = null!;
    public Job TargetJob { get; set; } = null!;
}
