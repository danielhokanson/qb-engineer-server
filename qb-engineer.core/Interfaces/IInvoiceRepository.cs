using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IInvoiceRepository
{
    Task<List<InvoiceListItemModel>> GetAllAsync(int? customerId, InvoiceStatus? status, CancellationToken ct);
    Task<Invoice?> FindAsync(int id, CancellationToken ct);
    Task<Invoice?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct);
    Task AddAsync(Invoice invoice, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
