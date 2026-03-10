namespace QBEngineer.Core.Models;

public record QcInspectionResponseModel(
    int Id,
    int? JobId,
    string? JobNumber,
    int? ProductionRunId,
    int? TemplateId,
    string? TemplateName,
    int InspectorId,
    string InspectorName,
    string? LotNumber,
    string Status,
    string? Notes,
    DateTime? CompletedAt,
    List<QcInspectionResultModel> Results,
    DateTime CreatedAt);
