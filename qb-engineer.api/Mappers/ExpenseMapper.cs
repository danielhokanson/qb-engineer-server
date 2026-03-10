using Riok.Mapperly.Abstractions;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Mappers;

[Mapper]
public static partial class ExpenseMapper
{
    /// <summary>
    /// Maps an Expense entity to an ExpenseResponseModel.
    /// User and job names must be provided separately as they require lookups.
    /// </summary>
    public static ExpenseResponseModel ToResponseModel(
        this Expense expense,
        string userName,
        string? jobNumber = null,
        string? approvedByName = null)
    {
        return new ExpenseResponseModel(
            Id: expense.Id,
            UserId: expense.UserId,
            UserName: userName,
            JobId: expense.JobId,
            JobNumber: jobNumber ?? expense.Job?.JobNumber,
            Amount: expense.Amount,
            Category: expense.Category,
            Description: expense.Description,
            ReceiptFileId: expense.ReceiptFileId,
            Status: expense.Status,
            ApprovedBy: expense.ApprovedBy,
            ApprovedByName: approvedByName,
            ApprovalNotes: expense.ApprovalNotes,
            ExpenseDate: expense.ExpenseDate,
            CreatedAt: expense.CreatedAt);
    }
}
