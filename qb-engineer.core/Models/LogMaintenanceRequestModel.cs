namespace QBEngineer.Core.Models;

public record LogMaintenanceRequestModel(
    decimal? HoursAtService,
    string? Notes,
    decimal? Cost);
