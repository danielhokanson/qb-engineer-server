namespace QBEngineer.Core.Models;

public record BoardJobCreatedEvent(
    int JobId,
    string JobNumber,
    string Title,
    int TrackTypeId,
    int StageId,
    string StageName,
    int BoardPosition);

public record BoardJobMovedEvent(
    int JobId,
    int FromStageId,
    int ToStageId,
    string ToStageName,
    int BoardPosition);

public record BoardJobUpdatedEvent(
    int JobId,
    JobDetailResponseModel Job);

public record BoardJobPositionChangedEvent(
    int JobId,
    int StageId,
    int NewPosition);

public record TimerStartedEvent(
    int UserId,
    TimeEntryResponseModel Entry);

public record TimerStoppedEvent(
    int UserId,
    TimeEntryResponseModel Entry);
