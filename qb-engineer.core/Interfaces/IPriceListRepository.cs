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

    // Entry-level CRUD (introduced for the PriceListEntry edit UI).
    Task<bool> PriceListExistsAsync(int priceListId, CancellationToken ct);
    Task<PagedResponse<PriceListEntryResponseModel>> GetEntriesAsync(
        int priceListId, string? search, int page, int pageSize, CancellationToken ct);
    Task<PriceListEntry?> FindEntryAsync(int entryId, CancellationToken ct);
    Task<PriceListEntry?> FindEntryWithPartAsync(int entryId, CancellationToken ct);
    Task AddEntryAsync(PriceListEntry entry, CancellationToken ct);
}
