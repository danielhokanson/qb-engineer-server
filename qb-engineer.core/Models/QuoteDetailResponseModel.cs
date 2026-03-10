namespace QBEngineer.Core.Models;

public record QuoteDetailResponseModel(
    int Id,
    string QuoteNumber,
    int CustomerId,
    string CustomerName,
    int? ShippingAddressId,
    string Status,
    DateTime? SentDate,
    DateTime? ExpirationDate,
    DateTime? AcceptedDate,
    string? Notes,
    decimal TaxRate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    int? SalesOrderId,
    string? SalesOrderNumber,
    List<QuoteLineResponseModel> Lines,
    DateTime CreatedAt,
    DateTime UpdatedAt);
