namespace QBEngineer.Core.Models;

public record QuoteListItemModel(
    int Id,
    string QuoteNumber,
    int CustomerId,
    string CustomerName,
    string Status,
    int LineCount,
    decimal Total,
    DateTime? ExpirationDate,
    DateTime CreatedAt);
