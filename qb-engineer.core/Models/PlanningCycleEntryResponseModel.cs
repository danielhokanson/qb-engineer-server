namespace QBEngineer.Core.Models;

public record PlanningCycleEntryResponseModel(
    int Id,
    int JobId,
    string JobNumber,
    string JobTitle,
    string? AssigneeName,
    string StageName,
    string? StageColor,
    string Priority,
    bool IsRolledOver,
    DateTime CommittedAt,
    DateTime? CompletedAt,
    int SortOrder);
