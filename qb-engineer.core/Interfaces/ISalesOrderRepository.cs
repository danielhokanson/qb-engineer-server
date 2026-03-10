using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ISalesOrderRepository
{
    Task<List<SalesOrderListItemModel>> GetAllAsync(int? customerId, SalesOrderStatus? status, CancellationToken ct);
    Task<SalesOrder?> FindAsync(int id, CancellationToken ct);
    Task<SalesOrder?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task<string> GenerateNextOrderNumberAsync(CancellationToken ct);
    Task AddAsync(SalesOrder order, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
