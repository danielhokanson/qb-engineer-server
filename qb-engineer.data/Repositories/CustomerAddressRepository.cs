using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class CustomerAddressRepository(AppDbContext db) : ICustomerAddressRepository
{
    public async Task<List<CustomerAddressResponseModel>> GetByCustomerAsync(int customerId, CancellationToken ct)
    {
        return await db.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .Select(a => new CustomerAddressResponseModel(
                a.Id,
                a.Label,
                a.AddressType.ToString(),
                a.Line1,
                a.Line2,
                a.City,
                a.State,
                a.PostalCode,
                a.Country,
                a.IsDefault))
            .ToListAsync(ct);
    }

    public async Task<CustomerAddress?> FindAsync(int id, CancellationToken ct)
    {
        return await db.CustomerAddresses.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task AddAsync(CustomerAddress address, CancellationToken ct)
    {
        await db.CustomerAddresses.AddAsync(address, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
