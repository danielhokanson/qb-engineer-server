namespace QBEngineer.Core.Entities;

public class Operation : BaseAuditableEntity
{
    public int PartId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public int? WorkCenterId { get; set; }
    public int? EstimatedMinutes { get; set; }
    public bool IsQcCheckpoint { get; set; }
    public string? QcCriteria { get; set; }

    public Part Part { get; set; } = null!;
    public Asset? WorkCenter { get; set; }
}
