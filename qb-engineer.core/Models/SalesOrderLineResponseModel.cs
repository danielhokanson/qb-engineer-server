namespace QBEngineer.Core.Models;

public record SalesOrderLineResponseModel(
    int Id,
    int? PartId,
    string? PartNumber,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int LineNumber,
    int ShippedQuantity,
    int RemainingQuantity,
    bool IsFullyShipped,
    string? Notes,
    List<SalesOrderLineJobModel> Jobs);
