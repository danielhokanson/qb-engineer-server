namespace QBEngineer.Core.Models;

public record CreateProcessStepRequestModel(
    int StepNumber,
    string Title,
    string? Instructions,
    int? WorkCenterId,
    int? EstimatedMinutes,
    bool IsQcCheckpoint,
    string? QcCriteria);
