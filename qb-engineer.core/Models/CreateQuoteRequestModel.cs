namespace QBEngineer.Core.Models;

public record CreateQuoteRequestModel(
    int CustomerId,
    int? ShippingAddressId,
    DateTime? ExpirationDate,
    string? Notes,
    decimal TaxRate,
    List<CreateQuoteLineModel> Lines);

public record CreateQuoteLineModel(
    int? PartId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    string? Notes);
