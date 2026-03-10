namespace QBEngineer.Core.Models;

public record PlanningCycleListItemModel(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int TotalJobs,
    int CompletedJobs,
    DateTime CreatedAt);
