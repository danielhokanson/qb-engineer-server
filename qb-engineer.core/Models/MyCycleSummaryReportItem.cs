namespace QBEngineer.Core.Models;

public record MyCycleSummaryReportItem(
    int CycleId,
    string CycleName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalEntries,
    int CompletedEntries,
    decimal CompletionRate,
    int RolledOverCount);
