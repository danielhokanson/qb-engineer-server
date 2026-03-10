namespace QBEngineer.Core.Models;

public record ShippingSummaryReportItem(
    string Status,
    int Count,
    int TotalItems,
    int OnTimeCount,
    int LateCount);
