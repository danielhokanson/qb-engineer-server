using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Services;

public class BackwardSchedulingService(AppDbContext db, IClock clock)
{
    /// <summary>
    /// Buffer days between milestones (configurable defaults).
    /// </summary>
    private const int ShippingBufferDays = 2;
    private const int QcBufferDays = 1;
    private const int DefaultProductionDays = 5;

    public async Task<BackwardSchedule> CalculateSchedule(int salesOrderLineId, CancellationToken ct)
    {
        var soLine = await db.SalesOrderLines
            .Include(l => l.SalesOrder)
            .Include(l => l.Part)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == salesOrderLineId, ct);

        if (soLine is null)
            throw new KeyNotFoundException($"SalesOrderLine {salesOrderLineId} not found");

        // Delivery date — from the SO's requested delivery date or a fallback
        var deliveryDate = soLine.SalesOrder.RequestedDeliveryDate ?? clock.UtcNow.AddDays(30);

        // Ship by = delivery - shipping buffer
        var shipBy = deliveryDate.AddDays(-ShippingBufferDays);

        // QC complete by = ship by - QC buffer
        var qcCompleteBy = shipBy.AddDays(-QcBufferDays);

        // Estimate production duration from operations or use default
        var productionDays = DefaultProductionDays;
        if (soLine.PartId.HasValue)
        {
            var operationCount = await db.Operations
                .CountAsync(o => o.PartId == soLine.PartId, ct);
            if (operationCount > 0)
                productionDays = Math.Max(operationCount, DefaultProductionDays);
        }

        // Production complete = QC complete
        var productionCompleteBy = qcCompleteBy;

        // Production start = production complete - production duration
        var productionStartBy = productionCompleteBy.AddDays(-productionDays);

        // Materials needed by = production start
        var materialsNeededBy = productionStartBy;

        // PO order by = materials needed - max lead time from Buy BOM entries
        var maxLeadTimeDays = 0;
        if (soLine.PartId.HasValue)
        {
            var buyLeadTimes = await db.BOMEntries
                .Where(b => b.ParentPartId == soLine.PartId && b.SourceType == BOMSourceType.Buy && b.LeadTimeDays.HasValue)
                .Select(b => b.LeadTimeDays!.Value)
                .ToListAsync(ct);

            if (buyLeadTimes.Count > 0)
                maxLeadTimeDays = buyLeadTimes.Max();
        }

        var poOrderBy = materialsNeededBy.AddDays(-maxLeadTimeDays);

        return new BackwardSchedule(
            DeliveryDate: deliveryDate,
            ShipBy: shipBy,
            QcCompleteBy: qcCompleteBy,
            ProductionCompleteBy: productionCompleteBy,
            ProductionStartBy: productionStartBy,
            MaterialsNeededBy: materialsNeededBy,
            PoOrderBy: poOrderBy);
    }
}

public record BackwardSchedule(
    DateTimeOffset DeliveryDate,
    DateTimeOffset ShipBy,
    DateTimeOffset QcCompleteBy,
    DateTimeOffset ProductionCompleteBy,
    DateTimeOffset ProductionStartBy,
    DateTimeOffset MaterialsNeededBy,
    DateTimeOffset PoOrderBy);
