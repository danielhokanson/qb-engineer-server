namespace QBEngineer.Core.Models;

public record ShipmentRequest(
    ShippingAddress FromAddress,
    ShippingAddress ToAddress,
    List<ShippingPackage> Packages,
    string? ServiceType);

public record ShippingAddress(
    string Name,
    string Street,
    string City,
    string State,
    string Zip,
    string Country);

public record ShippingPackage(
    decimal WeightLbs,
    decimal LengthIn,
    decimal WidthIn,
    decimal HeightIn);

public record ShippingRate(
    string CarrierId,
    string CarrierName,
    string ServiceName,
    decimal Price,
    int EstimatedDays);

public record ShippingLabel(
    string TrackingNumber,
    string LabelUrl,
    string CarrierName);

public record ShipmentTracking(
    string TrackingNumber,
    string Status,
    DateTime? EstimatedDelivery,
    List<TrackingEvent> Events);

public record TrackingEvent(
    DateTime Timestamp,
    string Location,
    string Description);
