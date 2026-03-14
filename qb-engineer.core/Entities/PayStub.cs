using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class PayStub : BaseAuditableEntity
{
    public int UserId { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public decimal GrossPay { get; set; }
    public decimal NetPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalTaxes { get; set; }
    public int? FileAttachmentId { get; set; }
    public PayrollDocumentSource Source { get; set; }
    public string? ExternalId { get; set; }

    public FileAttachment? FileAttachment { get; set; }
    public ICollection<PayStubDeduction> Deductions { get; set; } = [];
}
