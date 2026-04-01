namespace QBEngineer.Core.Models;

public record TrackingEvent(
    DateTimeOffset Timestamp,
    string Location,
    string Description);
