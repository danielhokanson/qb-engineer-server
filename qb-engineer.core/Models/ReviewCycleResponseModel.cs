using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ReviewCycleResponseModel(
    int Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    ReviewCycleStatus Status,
    string? Description,
    int ReviewCount);
