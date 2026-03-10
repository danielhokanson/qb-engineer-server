using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class PurchaseOrderRepository(AppDbContext db) : IPurchaseOrderRepository
{
    public async Task<List<PurchaseOrderListItemModel>> GetAllAsync(
        int? vendorId, int? jobId, PurchaseOrderStatus? status, CancellationToken ct)
    {
        var query = db.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Job)
            .Include(po => po.Lines)
            .AsQueryable();

        if (vendorId.HasValue)
            query = query.Where(po => po.VendorId == vendorId.Value);

        if (jobId.HasValue)
            query = query.Where(po => po.JobId == jobId.Value);

        if (status.HasValue)
            query = query.Where(po => po.Status == status.Value);

        return await query
            .OrderByDescending(po => po.CreatedAt)
            .Select(po => new PurchaseOrderListItemModel(
                po.Id,
                po.PONumber,
                po.VendorId,
                po.Vendor.CompanyName,
                po.JobId,
                po.Job != null ? po.Job.JobNumber : null,
                po.Status.ToString(),
                po.Lines.Count,
                po.Lines.Sum(l => l.OrderedQuantity),
                po.Lines.Sum(l => l.ReceivedQuantity),
                po.ExpectedDeliveryDate,
                po.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<PurchaseOrder?> FindAsync(int id, CancellationToken ct)
    {
        return await db.PurchaseOrders.FirstOrDefaultAsync(po => po.Id == id, ct);
    }

    public async Task<PurchaseOrder?> FindWithDetailsAsync(int id, CancellationToken ct)
    {
        return await db.PurchaseOrders
            .Include(po => po.Vendor)
            .Include(po => po.Job)
            .Include(po => po.Lines)
                .ThenInclude(l => l.Part)
            .Include(po => po.Lines)
                .ThenInclude(l => l.ReceivingRecords.Where(r => r.DeletedAt == null))
            .FirstOrDefaultAsync(po => po.Id == id, ct);
    }

    public async Task<PurchaseOrderLine?> FindLineAsync(int lineId, CancellationToken ct)
    {
        return await db.PurchaseOrderLines
            .Include(l => l.PurchaseOrder)
            .Include(l => l.Part)
            .FirstOrDefaultAsync(l => l.Id == lineId, ct);
    }

    public async Task<string> GenerateNextPONumberAsync(CancellationToken ct)
    {
        var lastPo = await db.PurchaseOrders
            .IgnoreQueryFilters()
            .OrderByDescending(po => po.Id)
            .Select(po => po.PONumber)
            .FirstOrDefaultAsync(ct);

        if (lastPo != null && lastPo.StartsWith("PO-") && int.TryParse(lastPo[3..], out var lastNum))
            return $"PO-{lastNum + 1:D5}";

        return "PO-00001";
    }

    public async Task AddAsync(PurchaseOrder po, CancellationToken ct)
    {
        await db.PurchaseOrders.AddAsync(po, ct);
    }

    public async Task AddReceivingRecordAsync(ReceivingRecord record, CancellationToken ct)
    {
        await db.ReceivingRecords.AddAsync(record, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
