namespace QBEngineer.Core.Models;

public record ArAgingReportItem(
    int InvoiceId,
    string InvoiceNumber,
    string CustomerName,
    DateTime InvoiceDate,
    DateTime DueDate,
    decimal Total,
    decimal AmountPaid,
    decimal BalanceDue,
    int DaysOverdue,
    string AgingBucket);
