namespace QBEngineer.Core.Models;

public record UpdateProcessStepRequestModel(
    int? StepNumber,
    string? Title,
    string? Instructions,
    int? WorkCenterId,
    int? EstimatedMinutes,
    bool? IsQcCheckpoint,
    string? QcCriteria);
