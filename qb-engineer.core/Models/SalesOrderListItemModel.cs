namespace QBEngineer.Core.Models;

public record SalesOrderListItemModel(
    int Id,
    string OrderNumber,
    int CustomerId,
    string CustomerName,
    string Status,
    string? CustomerPO,
    int LineCount,
    decimal Total,
    DateTime? RequestedDeliveryDate,
    DateTime CreatedAt);
