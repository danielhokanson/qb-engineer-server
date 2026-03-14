using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PayStubResponseModel(
    int Id,
    int UserId,
    DateTime PayPeriodStart,
    DateTime PayPeriodEnd,
    DateTime PayDate,
    decimal GrossPay,
    decimal NetPay,
    decimal TotalDeductions,
    decimal TotalTaxes,
    int? FileAttachmentId,
    PayrollDocumentSource Source,
    string? ExternalId,
    List<PayStubDeductionResponseModel> Deductions);
