namespace QBEngineer.Core.Models;

public record CreatePaymentRequestModel(
    int CustomerId,
    string Method,
    decimal Amount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Notes,
    List<CreatePaymentApplicationModel>? Applications);

public record CreatePaymentApplicationModel(
    int InvoiceId,
    decimal Amount);
