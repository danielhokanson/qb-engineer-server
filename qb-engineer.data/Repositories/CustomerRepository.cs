using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
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

    public async Task<List<CustomerListItemModel>> GetAllAsync(string? search, bool? isActive, CancellationToken ct)
    {
        var query = db.Customers.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerListItemModel(
                c.Id,
                c.Name,
                c.CompanyName,
                c.Email,
                c.Phone,
                c.IsActive,
                c.Contacts.Count(ct => ct.DeletedAt == null),
                c.Jobs.Count(j => j.DeletedAt == null),
                c.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Customer?> FindAsync(int id, CancellationToken ct)
    {
        return await db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.Customers
            .Include(c => c.Contacts.Where(ct => ct.DeletedAt == null))
            .Include(c => c.Jobs.Where(j => j.DeletedAt == null))
                .ThenInclude(j => j.CurrentStage)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Customer customer, CancellationToken ct)
    {
        await db.Customers.AddAsync(customer, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
