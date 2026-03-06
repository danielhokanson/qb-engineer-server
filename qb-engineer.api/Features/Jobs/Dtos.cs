namespace QBEngineer.Api.Features.Jobs;

public record JobListDto(
    int Id,
    string JobNumber,
    string Title,
    string StageName,
    string StageColor,
    string? AssigneeInitials,
    string? AssigneeColor,
    string PriorityName,
    DateTime? DueDate,
    bool IsOverdue,
    string? CustomerName);

public record JobDetailDto(
    int Id,
    string JobNumber,
    string Title,
    string? Description,
    int TrackTypeId,
    string TrackTypeName,
    int CurrentStageId,
    string StageName,
    string StageColor,
    int? AssigneeId,
    string? AssigneeInitials,
    string? AssigneeName,
    string? AssigneeColor,
    string Priority,
    int? CustomerId,
    string? CustomerName,
    DateTime? DueDate,
    DateTime? StartDate,
    DateTime? CompletedDate,
    bool IsArchived,
    int BoardPosition,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ActivityDto(
    int Id,
    string Action,
    string? FieldName,
    string? OldValue,
    string? NewValue,
    string Description,
    string? UserInitials,
    string? UserName,
    DateTime CreatedAt);
