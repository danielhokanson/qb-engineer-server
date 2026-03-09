using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ICustomerRepository
{
    Task<List<CustomerResponseModel>> GetAllActiveAsync(CancellationToken ct);
    Task<List<CustomerListItemModel>> GetAllAsync(string? search, bool? isActive, CancellationToken ct);
    Task<Customer?> FindAsync(int id, CancellationToken ct);
    Task<Customer?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
