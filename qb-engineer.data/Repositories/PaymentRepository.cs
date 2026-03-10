using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class PaymentRepository(AppDbContext db) : IPaymentRepository
{
    public async Task<List<PaymentListItemModel>> GetAllAsync(int? customerId, CancellationToken ct)
    {
        var query = db.Payments
            .Include(p => p.Customer)
            .Include(p => p.Applications)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == customerId.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentListItemModel(
                p.Id,
                p.PaymentNumber,
                p.CustomerId,
                p.Customer.Name,
                p.Method.ToString(),
                p.Amount,
                p.Applications.Sum(a => a.Amount),
                p.Amount - p.Applications.Sum(a => a.Amount),
                p.PaymentDate,
                p.ReferenceNumber,
                p.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Payment?> FindAsync(int id, CancellationToken ct)
    {
        return await db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Payment?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.Payments
            .Include(p => p.Customer)
            .Include(p => p.Applications)
                .ThenInclude(a => a.Invoice)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<string> GenerateNextPaymentNumberAsync(CancellationToken ct)
    {
        var last = await db.Payments
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.Id)
            .Select(p => p.PaymentNumber)
            .FirstOrDefaultAsync(ct);

        if (last != null && last.StartsWith("PMT-") && int.TryParse(last[4..], out var lastNum))
            return $"PMT-{lastNum + 1:D5}";

        return "PMT-00001";
    }

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        await db.Payments.AddAsync(payment, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
