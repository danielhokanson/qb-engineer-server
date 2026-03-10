namespace QBEngineer.Core.Models;

public record MaintenanceLogResponseModel(
    int Id,
    int MaintenanceScheduleId,
    string PerformedByName,
    DateTime PerformedAt,
    decimal? HoursAtService,
    string? Notes,
    decimal? Cost);
