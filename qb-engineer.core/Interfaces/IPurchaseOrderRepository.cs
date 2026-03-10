using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrderListItemModel>> GetAllAsync(int? vendorId, int? jobId, PurchaseOrderStatus? status, CancellationToken ct);
    Task<PurchaseOrder?> FindAsync(int id, CancellationToken ct);
    Task<PurchaseOrder?> FindWithDetailsAsync(int id, CancellationToken ct);
    Task<PurchaseOrderLine?> FindLineAsync(int lineId, CancellationToken ct);
    Task<string> GenerateNextPONumberAsync(CancellationToken ct);
    Task AddAsync(PurchaseOrder po, CancellationToken ct);
    Task AddReceivingRecordAsync(ReceivingRecord record, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
