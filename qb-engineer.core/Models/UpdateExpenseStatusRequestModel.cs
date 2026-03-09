using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdateExpenseStatusRequestModel(
    ExpenseStatus Status,
    string? ApprovalNotes);
