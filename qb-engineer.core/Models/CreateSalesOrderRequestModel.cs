namespace QBEngineer.Core.Models;

public record CreateSalesOrderRequestModel(
    int CustomerId,
    int? QuoteId,
    int? ShippingAddressId,
    int? BillingAddressId,
    string? CreditTerms,
    DateTime? RequestedDeliveryDate,
    string? CustomerPO,
    string? Notes,
    decimal TaxRate,
    List<CreateSalesOrderLineModel> Lines);

public record CreateSalesOrderLineModel(
    int? PartId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    string? Notes);
