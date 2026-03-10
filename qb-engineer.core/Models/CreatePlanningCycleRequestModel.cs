namespace QBEngineer.Core.Models;

public record CreatePlanningCycleRequestModel(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string? Goals,
    int? DurationDays);
