namespace QBEngineer.Core.Models;

public record CreateExpenseRequestModel(
    decimal Amount,
    string Category,
    string Description,
    int? JobId,
    string? ReceiptFileId,
    DateTime ExpenseDate);
