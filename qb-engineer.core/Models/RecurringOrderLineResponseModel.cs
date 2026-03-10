namespace QBEngineer.Core.Models;

public record RecurringOrderLineResponseModel(
    int Id,
    int PartId,
    string PartNumber,
    string Description,
    int Quantity,
    decimal UnitPrice,
    int LineNumber);
