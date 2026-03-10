using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IVendorRepository
{
    Task<List<VendorResponseModel>> GetAllActiveAsync(CancellationToken ct);
    Task<List<VendorListItemModel>> GetAllAsync(string? search, bool? isActive, CancellationToken ct);
    Task<Vendor?> FindAsync(int id, CancellationToken ct);
    Task<Vendor?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task AddAsync(Vendor vendor, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
