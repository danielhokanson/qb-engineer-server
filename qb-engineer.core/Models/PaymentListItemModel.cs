namespace QBEngineer.Core.Models;

public record PaymentListItemModel(
    int Id,
    string PaymentNumber,
    int CustomerId,
    string CustomerName,
    string Method,
    decimal Amount,
    decimal AppliedAmount,
    decimal UnappliedAmount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    DateTime CreatedAt);
