namespace QBEngineer.Core.Models;

public record ShipmentLineResponseModel(
    int Id,
    int SalesOrderLineId,
    string Description,
    int Quantity,
    string? Notes);
