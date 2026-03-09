using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ExpenseResponseModel(
    int Id,
    int UserId,
    string UserName,
    int? JobId,
    string? JobNumber,
    decimal Amount,
    string Category,
    string Description,
    string? ReceiptFileId,
    ExpenseStatus Status,
    int? ApprovedBy,
    string? ApprovedByName,
    string? ApprovalNotes,
    DateTime ExpenseDate,
    DateTime CreatedAt);

public record CreateExpenseRequestModel(
    decimal Amount,
    string Category,
    string Description,
    int? JobId,
    string? ReceiptFileId,
    DateTime ExpenseDate);

public record UpdateExpenseStatusRequestModel(
    ExpenseStatus Status,
    string? ApprovalNotes);
