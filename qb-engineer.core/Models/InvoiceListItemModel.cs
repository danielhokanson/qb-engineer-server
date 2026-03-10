namespace QBEngineer.Core.Models;

public record InvoiceListItemModel(
    int Id,
    string InvoiceNumber,
    int CustomerId,
    string CustomerName,
    string Status,
    DateTime InvoiceDate,
    DateTime DueDate,
    decimal Total,
    decimal AmountPaid,
    decimal BalanceDue,
    DateTime CreatedAt);
