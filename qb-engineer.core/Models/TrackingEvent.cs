namespace QBEngineer.Core.Models;

public record TrackingEvent(
    DateTime Timestamp,
    string Location,
    string Description);
