namespace QBEngineer.Core.Models;

public record CycleCountResponseModel(
    int Id,
    int LocationId,
    string LocationName,
    int CountedById,
    string CountedByName,
    DateTime CountedAt,
    string Status,
    string? Notes,
    List<CycleCountLineResponseModel> Lines,
    DateTime CreatedAt);
