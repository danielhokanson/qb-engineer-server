namespace QBEngineer.Core.Models;

public record AccountingTimeActivity(
    string EmployeeExternalId,
    string? CustomerExternalId,
    decimal Hours,
    DateTime Date,
    string? Description,
    string? ServiceItemExternalId);
