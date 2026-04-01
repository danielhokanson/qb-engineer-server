namespace QBEngineer.Core.Models;

public record UpdateQuoteRequestModel(
    int? ShippingAddressId,
    DateTimeOffset? ExpirationDate,
    string? Notes,
    decimal? TaxRate);
