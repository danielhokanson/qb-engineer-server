namespace QBEngineer.Core.Models;

public record PaymentDetailResponseModel(
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
    string? Notes,
    List<PaymentApplicationResponseModel> Applications,
    DateTime CreatedAt,
    DateTime UpdatedAt);
