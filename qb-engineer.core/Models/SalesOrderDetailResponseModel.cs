namespace QBEngineer.Core.Models;

public record SalesOrderDetailResponseModel(
    int Id,
    string OrderNumber,
    int CustomerId,
    string CustomerName,
    int? QuoteId,
    string? QuoteNumber,
    int? ShippingAddressId,
    int? BillingAddressId,
    string Status,
    string? CreditTerms,
    DateTime? ConfirmedDate,
    DateTime? RequestedDeliveryDate,
    string? CustomerPO,
    string? Notes,
    decimal TaxRate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    List<SalesOrderLineResponseModel> Lines,
    DateTime CreatedAt,
    DateTime UpdatedAt);
