namespace QBEngineer.Core.Models;

public record CreatePlanningCycleRequestModel(
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? Goals,
    int? DurationDays);
