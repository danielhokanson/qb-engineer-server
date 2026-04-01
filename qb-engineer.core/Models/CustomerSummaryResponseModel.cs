namespace QBEngineer.Core.Models;

public record CustomerSummaryResponseModel(
    int Id,
    string Name,
    string? CompanyName,
    string? Email,
    string? Phone,
    bool IsActive,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int EstimateCount,
    int QuoteCount,
    int OrderCount,
    int ActiveJobCount,
    int OpenInvoiceCount,
    decimal OpenInvoiceTotal,
    decimal YtdRevenue);
