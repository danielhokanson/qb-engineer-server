namespace QBEngineer.Core.Models;

public record RdReportItem(
    int JobId,
    string JobNumber,
    string Title,
    int IterationCount,
    decimal TotalHours,
    string CurrentStage,
    string? AssigneeName,
    DateTime? StartDate,
    DateTime? CompletedDate);
