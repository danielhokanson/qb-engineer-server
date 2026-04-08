namespace QBEngineer.Core.Models;

public record OperationMaterialResponseModel(
    int Id,
    int OperationId,
    int BomEntryId,
    string ChildPartNumber,
    string ChildPartDescription,
    decimal Quantity,
    string? Notes);
