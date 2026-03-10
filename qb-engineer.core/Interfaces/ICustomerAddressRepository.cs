using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ICustomerAddressRepository
{
    Task<List<CustomerAddressResponseModel>> GetByCustomerAsync(int customerId, CancellationToken ct);
    Task<CustomerAddress?> FindAsync(int id, CancellationToken ct);
    Task AddAsync(CustomerAddress address, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
