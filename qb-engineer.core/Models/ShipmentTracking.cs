namespace QBEngineer.Core.Models;

public record ShipmentTracking(
    string TrackingNumber,
    string Status,
    DateTime? EstimatedDelivery,
    List<TrackingEvent> Events);
