namespace QBEngineer.Core.Models;

public record UpdateQuoteRequestModel(
    int? ShippingAddressId,
    DateTime? ExpirationDate,
    string? Notes,
    decimal? TaxRate);
