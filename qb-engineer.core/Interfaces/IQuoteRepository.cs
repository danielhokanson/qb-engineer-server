using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IQuoteRepository
{
    Task<List<QuoteListItemModel>> GetAllAsync(int? customerId, QuoteStatus? status, CancellationToken ct);
    Task<Quote?> FindAsync(int id, CancellationToken ct);
    Task<Quote?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task<string> GenerateNextQuoteNumberAsync(CancellationToken ct);
    Task AddAsync(Quote quote, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
