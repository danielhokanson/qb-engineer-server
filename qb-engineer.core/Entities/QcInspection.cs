namespace QBEngineer.Core.Entities;

public class QcInspection : BaseAuditableEntity
{
    public int? JobId { get; set; }
    public int? ProductionRunId { get; set; }
    public int? TemplateId { get; set; }
    public int InspectorId { get; set; }
    public string? LotNumber { get; set; }
    public string Status { get; set; } = "InProgress";
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Job? Job { get; set; }
    public ProductionRun? ProductionRun { get; set; }
    public QcChecklistTemplate? Template { get; set; }
    public ICollection<QcInspectionResult> Results { get; set; } = new List<QcInspectionResult>();
}
