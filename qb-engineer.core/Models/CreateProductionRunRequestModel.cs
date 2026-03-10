namespace QBEngineer.Core.Models;

public record CreateProductionRunRequestModel(
    int PartId,
    int TargetQuantity,
    int? OperatorId,
    string? Notes);
