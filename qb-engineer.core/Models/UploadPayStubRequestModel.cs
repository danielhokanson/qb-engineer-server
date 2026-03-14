namespace QBEngineer.Core.Models;

public record UploadPayStubRequestModel(
    DateTime PayPeriodStart,
    DateTime PayPeriodEnd,
    DateTime PayDate,
    decimal GrossPay,
    decimal NetPay,
    int FileAttachmentId);
