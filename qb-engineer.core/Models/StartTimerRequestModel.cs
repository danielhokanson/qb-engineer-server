namespace QBEngineer.Core.Models;

public record StartTimerRequestModel(
    int? JobId,
    string? Category,
    string? Notes);
