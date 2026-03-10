namespace QBEngineer.Core.Models;

public record PlanningCycleDetailResponseModel(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Goals,
    string Status,
    int DurationDays,
    List<PlanningCycleEntryResponseModel> Entries,
    DateTime CreatedAt,
    DateTime UpdatedAt);
