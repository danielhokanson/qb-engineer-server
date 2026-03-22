namespace QBEngineer.Core.Models;

public record MaintenanceLogListItemResponseModel(
    int Id,
    string ScheduleName,
    DateTime PerformedAt,
    string PerformedByName,
    decimal? HoursSpent,
    string? Notes,
    decimal? Cost);
