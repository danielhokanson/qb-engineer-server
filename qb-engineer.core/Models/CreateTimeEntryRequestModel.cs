namespace QBEngineer.Core.Models;

public record CreateTimeEntryRequestModel(
    int? JobId,
    DateOnly Date,
    int DurationMinutes,
    string? Category,
    string? Notes);
