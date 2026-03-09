namespace QBEngineer.Core.Models;

public record OverdueJobReportItem(
    int Id,
    string JobNumber,
    string Title,
    DateTime DueDate,
    int DaysOverdue,
    string? AssigneeName);
