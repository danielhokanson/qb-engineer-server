namespace QBEngineer.Core.Models;

public record ProcessStepResponseModel(
    int Id,
    int PartId,
    int StepNumber,
    string Title,
    string? Instructions,
    int? WorkCenterId,
    string? WorkCenterName,
    int? EstimatedMinutes,
    bool IsQcCheckpoint,
    string? QcCriteria,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
