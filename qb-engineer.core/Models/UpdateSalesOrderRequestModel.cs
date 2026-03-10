namespace QBEngineer.Core.Models;

public record UpdateSalesOrderRequestModel(
    int? ShippingAddressId,
    int? BillingAddressId,
    string? CreditTerms,
    DateTime? RequestedDeliveryDate,
    string? CustomerPO,
    string? Notes,
    decimal? TaxRate);
