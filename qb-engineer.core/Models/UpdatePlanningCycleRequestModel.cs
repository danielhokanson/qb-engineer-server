namespace QBEngineer.Core.Models;

public record UpdatePlanningCycleRequestModel(
    string? Name,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Goals);
