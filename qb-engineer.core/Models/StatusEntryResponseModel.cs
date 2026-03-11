namespace QBEngineer.Core.Models;

public record StatusEntryResponseModel(
    int Id,
    string EntityType,
    int EntityId,
    string StatusCode,
    string StatusLabel,
    string Category,
    DateTime StartedAt,
    DateTime? EndedAt,
    string? Notes,
    int? SetById,
    string? SetByName,
    DateTime CreatedAt);
