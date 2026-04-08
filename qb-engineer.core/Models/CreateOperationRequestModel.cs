namespace QBEngineer.Core.Models;

public record CreateOperationRequestModel(
    int StepNumber,
    string Title,
    string? Instructions,
    int? WorkCenterId,
    int? EstimatedMinutes,
    bool IsQcCheckpoint,
    string? QcCriteria,
    int? ReferencedOperationId);
