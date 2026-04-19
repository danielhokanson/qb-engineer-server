namespace QBEngineer.Core.Models;

public record SalesOrderShipmentLineModel(
    int Id,
    int? PartId,
    string? PartNumber,
    int Quantity,
    string? Notes,
    int? SalesOrderLineId);
