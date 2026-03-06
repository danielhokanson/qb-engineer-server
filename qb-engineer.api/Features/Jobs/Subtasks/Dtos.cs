namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record SubtaskDto(
    int Id,
    int JobId,
    string Text,
    bool IsCompleted,
    int? AssigneeId,
    int SortOrder,
    DateTime? CompletedAt);
