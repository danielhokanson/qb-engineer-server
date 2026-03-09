using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

public class Expense : BaseAuditableEntity
{
    public int UserId { get; set; }
    public int? JobId { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReceiptFileId { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
    public int? ApprovedBy { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? ExternalExpenseId { get; set; }
    public DateTime ExpenseDate { get; set; }

    public Job? Job { get; set; }
}
