using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class InvoiceRepository(AppDbContext db) : IInvoiceRepository
{
    public async Task<List<InvoiceListItemModel>> GetAllAsync(
        int? customerId, InvoiceStatus? status, CancellationToken ct)
    {
        var query = db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Include(i => i.PaymentApplications)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvoiceListItemModel(
                i.Id,
                i.InvoiceNumber,
                i.CustomerId,
                i.Customer.Name,
                i.Status.ToString(),
                i.InvoiceDate,
                i.DueDate,
                i.Lines.Sum(l => l.Quantity * l.UnitPrice) * (1 + i.TaxRate),
                i.PaymentApplications.Sum(pa => pa.Amount),
                i.Lines.Sum(l => l.Quantity * l.UnitPrice) * (1 + i.TaxRate) - i.PaymentApplications.Sum(pa => pa.Amount),
                i.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Invoice?> FindAsync(int id, CancellationToken ct)
    {
        return await db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<Invoice?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Shipment)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Part)
            .Include(i => i.PaymentApplications)
                .ThenInclude(pa => pa.Payment)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct)
    {
        var last = await db.Invoices
            .IgnoreQueryFilters()
            .OrderByDescending(i => i.Id)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(ct);

        if (last != null && last.StartsWith("INV-") && int.TryParse(last[4..], out var lastNum))
            return $"INV-{lastNum + 1:D5}";

        return "INV-00001";
    }

    public async Task AddAsync(Invoice invoice, CancellationToken ct)
    {
        await db.Invoices.AddAsync(invoice, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
