using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IPriceListRepository
{
    Task<List<PriceListListItemModel>> GetAllAsync(int? customerId, CancellationToken ct);
    Task<PriceList?> FindAsync(int id, CancellationToken ct);
    Task<PriceList?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task AddAsync(PriceList priceList, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
