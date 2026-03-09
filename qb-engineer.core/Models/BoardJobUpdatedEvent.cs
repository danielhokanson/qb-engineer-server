namespace QBEngineer.Core.Models;

public record BoardJobUpdatedEvent(
    int JobId,
    JobDetailResponseModel Job);
