namespace QBEngineer.Core.Models;

public record LeadSalesReportItem(
    int NewLeads,
    int ConvertedLeads,
    decimal ConversionRate,
    int TotalQuotes,
    int TotalSalesOrders,
    decimal TotalSalesOrderValue,
    DateTime PeriodStart,
    DateTime PeriodEnd);
