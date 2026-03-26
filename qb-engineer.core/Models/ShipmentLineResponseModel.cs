namespace QBEngineer.Core.Models;

public record ShipmentLineResponseModel(
    int Id,
    int? SalesOrderLineId,
    int? PartId,
    string Description,
    int Quantity,
    string? Notes);
