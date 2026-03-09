using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IExpenseRepository
{
    Task<List<ExpenseResponseModel>> GetExpensesAsync(int? userId, ExpenseStatus? status, string? search, CancellationToken ct);
    Task<ExpenseResponseModel?> GetByIdAsync(int id, CancellationToken ct);
    Task<Expense?> FindAsync(int id, CancellationToken ct);
    Task AddAsync(Expense expense, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
