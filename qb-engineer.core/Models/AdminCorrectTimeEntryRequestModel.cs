namespace QBEngineer.Core.Models;

public record AdminCorrectTimeEntryRequestModel(
    int? JobId,
    DateOnly? Date,
    int? DurationMinutes,
    string? Category,
    string? Notes,
    string Reason);
