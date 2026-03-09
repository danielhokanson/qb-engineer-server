using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public async Task<List<CustomerResponseModel>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CustomerResponseModel(c.Id, c.Name))
            .ToListAsync(ct);
    }
}
