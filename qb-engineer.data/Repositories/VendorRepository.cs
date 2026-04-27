using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class VendorRepository(AppDbContext db) : IVendorRepository
{
    public async Task<List<VendorResponseModel>> GetAllActiveAsync(CancellationToken ct)
    {
        // Phase 3 H2 / WU-12: dropdown returns ALL vendors (active first,
        // then inactive) so the UI can render inactive entries greyed-out
        // with a "(deactivated)" suffix. The UI defaults to filtering
        // active-only and exposes a "Show inactive" toggle. The new
        // IsActive flag in the projection drives that.
        return await db.Vendors
            .OrderByDescending(v => v.IsActive)
            .ThenBy(v => v.CompanyName)
            .Select(v => new VendorResponseModel(v.Id, v.CompanyName, v.IsActive))
            .ToListAsync(ct);
    }

    public async Task<List<VendorListItemModel>> GetAllAsync(string? search, bool? isActive, CancellationToken ct)
    {
        var query = db.Vendors.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(v => v.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(v =>
                v.CompanyName.ToLower().Contains(term) ||
                (v.ContactName != null && v.ContactName.ToLower().Contains(term)) ||
                (v.Email != null && v.Email.ToLower().Contains(term)) ||
                (v.Phone != null && v.Phone.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(v => v.CompanyName)
            .Select(v => new VendorListItemModel(
                v.Id,
                v.CompanyName,
                v.ContactName,
                v.Email,
                v.Phone,
                v.IsActive,
                v.PurchaseOrders.Count(po => po.DeletedAt == null),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Vendor?> FindAsync(int id, CancellationToken ct)
    {
        return await db.Vendors.FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<Vendor?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.Vendors
            .Include(v => v.PurchaseOrders.Where(po => po.DeletedAt == null))
                .ThenInclude(po => po.Job)
            .Include(v => v.PurchaseOrders.Where(po => po.DeletedAt == null))
                .ThenInclude(po => po.Lines)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task AddAsync(Vendor vendor, CancellationToken ct)
    {
        await db.Vendors.AddAsync(vendor, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
