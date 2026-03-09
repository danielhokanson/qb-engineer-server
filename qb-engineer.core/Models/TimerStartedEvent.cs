namespace QBEngineer.Core.Models;

public record TimerStartedEvent(
    int UserId,
    TimeEntryResponseModel Entry);
