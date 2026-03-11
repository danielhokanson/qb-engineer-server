using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Job : BaseAuditableEntity
{
    public string JobNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrackTypeId { get; set; }
    public int CurrentStageId { get; set; }
    public int? AssigneeId { get; set; }
    public JobPriority Priority { get; set; } = JobPriority.Normal;
    public int? CustomerId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsArchived { get; set; }
    public int BoardPosition { get; set; }
    public int? PartId { get; set; }
    public int? ParentJobId { get; set; }
    public int? SalesOrderLineId { get; set; }

    // Accounting integration
    public string? ExternalId { get; set; }
    public string? ExternalRef { get; set; }
    public string? Provider { get; set; }

    // R&D iteration tracking
    public int IterationCount { get; set; }
    public string? IterationNotes { get; set; }

    // Internal projects
    public bool IsInternal { get; set; }
    public int? InternalProjectTypeId { get; set; }

    // Disposition
    public JobDisposition? Disposition { get; set; }
    public string? DispositionNotes { get; set; }
    public DateTime? DispositionAt { get; set; }

    // Custom fields (JSONB)
    public string? CustomFieldValues { get; set; }

    // Navigation
    public Part? Part { get; set; }
    public Job? ParentJob { get; set; }
    public ICollection<Job> ChildJobs { get; set; } = [];
    public TrackType TrackType { get; set; } = null!;
    public JobStage CurrentStage { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<JobSubtask> Subtasks { get; set; } = [];
    public ICollection<JobActivityLog> ActivityLogs { get; set; } = [];
    public ICollection<PlanningCycleEntry> PlanningCycleEntries { get; set; } = [];
    public SalesOrderLine? SalesOrderLine { get; set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
    public ICollection<JobPart> JobParts { get; set; } = [];
}
