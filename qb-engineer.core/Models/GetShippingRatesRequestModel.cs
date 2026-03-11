namespace QBEngineer.Core.Models;

public record GetShippingRatesRequestModel(
    ShippingAddress FromAddress,
    ShippingAddress ToAddress,
    List<ShippingPackage> Packages,
    string? ServiceType);
