namespace QBEngineer.Core.Models;

public record CycleReviewReportItem(
    int CycleId,
    string CycleName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalEntries,
    int CompletedEntries,
    decimal CompletionRate,
    int RolledOverCount);
