namespace QBEngineer.Core.Models;

public record CreateOperationMaterialRequestModel(
    int BomEntryId,
    decimal Quantity,
    string? Notes);
