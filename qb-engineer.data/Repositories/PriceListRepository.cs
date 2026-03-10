using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class PriceListRepository(AppDbContext db) : IPriceListRepository
{
    public async Task<List<PriceListListItemModel>> GetAllAsync(int? customerId, CancellationToken ct)
    {
        var query = db.PriceLists
            .Include(pl => pl.Customer)
            .Include(pl => pl.Entries)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(pl => pl.CustomerId == customerId.Value);

        return await query
            .OrderByDescending(pl => pl.IsDefault)
            .ThenBy(pl => pl.Name)
            .Select(pl => new PriceListListItemModel(
                pl.Id,
                pl.Name,
                pl.Description,
                pl.CustomerId,
                pl.Customer != null ? pl.Customer.Name : null,
                pl.IsDefault,
                pl.IsActive,
                pl.Entries.Count,
                pl.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<PriceList?> FindAsync(int id, CancellationToken ct)
    {
        return await db.PriceLists.FirstOrDefaultAsync(pl => pl.Id == id, ct);
    }

    public async Task<PriceList?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.PriceLists
            .Include(pl => pl.Customer)
            .Include(pl => pl.Entries)
                .ThenInclude(e => e.Part)
            .FirstOrDefaultAsync(pl => pl.Id == id, ct);
    }

    public async Task AddAsync(PriceList priceList, CancellationToken ct)
    {
        await db.PriceLists.AddAsync(priceList, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
