namespace QBEngineer.Core.Models;

public record ProductionRunResponseModel(
    int Id,
    int JobId,
    string JobNumber,
    int PartId,
    string PartNumber,
    string PartDescription,
    int? OperatorId,
    string? OperatorName,
    string RunNumber,
    int TargetQuantity,
    int CompletedQuantity,
    int ScrapQuantity,
    string Status,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Notes,
    decimal? SetupTimeMinutes,
    decimal? RunTimeMinutes,
    decimal YieldPercentage);
