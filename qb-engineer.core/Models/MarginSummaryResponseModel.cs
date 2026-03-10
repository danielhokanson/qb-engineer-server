namespace QBEngineer.Core.Models;

public record MarginSummaryResponseModel(
    decimal TotalRevenue,
    decimal TotalCost,
    decimal TotalMargin,
    decimal AverageMarginPercentage,
    int JobCount);
