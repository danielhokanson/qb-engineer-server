namespace QBEngineer.Core.Models;

public record LotTraceInspectionModel(
    int Id,
    string Status,
    string InspectorName,
    DateTime CreatedAt);
