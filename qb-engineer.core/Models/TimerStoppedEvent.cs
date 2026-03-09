namespace QBEngineer.Core.Models;

public record TimerStoppedEvent(
    int UserId,
    TimeEntryResponseModel Entry);
