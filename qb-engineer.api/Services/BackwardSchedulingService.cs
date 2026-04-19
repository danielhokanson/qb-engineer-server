using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Services;

public class BackwardSchedulingService(AppDbContext db, IClock clock) : IBackwardSchedulingService
{
    private const int ShippingBufferDays = 2;
    private const int QcBufferDays = 1;
    private const int DefaultProductionDays = 5;
    private const int DefaultLeadTimeDays = 7;
    private const int HoursPerDay = 8;

    public async Task<BackwardSchedule> CalculateSchedule(int salesOrderLineId, CancellationToken ct)
    {
        var soLine = await db.SalesOrderLines
            .Include(l => l.SalesOrder)
            .Include(l => l.Part)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == salesOrderLineId, ct);

        if (soLine is null)
            throw new KeyNotFoundException($"SalesOrderLine {salesOrderLineId} not found");

        var deliveryDate = soLine.SalesOrder.RequestedDeliveryDate ?? clock.UtcNow.AddDays(30);
        var shipBy = deliveryDate.AddDays(-ShippingBufferDays);
        var qcCompleteBy = shipBy.AddDays(-QcBufferDays);

        var productionDays = await CalculateProductionDaysAsync(soLine.PartId, ct);
        var productionCompleteBy = qcCompleteBy;
        var productionStartBy = productionCompleteBy.AddDays(-productionDays);
        var materialsNeededBy = productionStartBy;

        var maxLeadTimeDays = await CalculateMaxLeadTimeDaysAsync(soLine.PartId, ct);
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

    public async Task<List<ScheduleMilestone>> CalculateMilestonesAsync(int salesOrderLineId, CancellationToken ct = default)
    {
        var schedule = await CalculateSchedule(salesOrderLineId, ct);
        var now = clock.UtcNow;

        return
        [
            CreateMilestone(salesOrderLineId, "delivery", schedule.DeliveryDate, now),
            CreateMilestone(salesOrderLineId, "ship_by", schedule.ShipBy, now),
            CreateMilestone(salesOrderLineId, "qc_complete_by", schedule.QcCompleteBy, now),
            CreateMilestone(salesOrderLineId, "production_complete_by", schedule.ProductionCompleteBy, now),
            CreateMilestone(salesOrderLineId, "production_start_by", schedule.ProductionStartBy, now),
            CreateMilestone(salesOrderLineId, "materials_needed_by", schedule.MaterialsNeededBy, now),
            CreateMilestone(salesOrderLineId, "po_order_by", schedule.PoOrderBy, now),
        ];
    }

    private async Task<int> CalculateProductionDaysAsync(int? partId, CancellationToken ct)
    {
        if (!partId.HasValue)
            return DefaultProductionDays;

        var totalMinutes = await db.Operations
            .Where(o => o.PartId == partId.Value)
            .SumAsync(o => (o.EstimatedMinutes ?? 0) + (int)o.SetupMinutes, ct);

        if (totalMinutes <= 0)
            return DefaultProductionDays;

        var totalHours = totalMinutes / 60.0;
        var days = (int)Math.Ceiling(totalHours / HoursPerDay);
        return Math.Max(days, DefaultProductionDays);
    }

    private async Task<int> CalculateMaxLeadTimeDaysAsync(int? partId, CancellationToken ct)
    {
        if (!partId.HasValue)
            return DefaultLeadTimeDays;

        var buyLeadTimes = await db.BOMEntries
            .Where(b => b.ParentPartId == partId.Value && b.SourceType == BOMSourceType.Buy && b.LeadTimeDays.HasValue)
            .Select(b => b.LeadTimeDays!.Value)
            .ToListAsync(ct);

        return buyLeadTimes.Count > 0 ? buyLeadTimes.Max() : DefaultLeadTimeDays;
    }

    private static ScheduleMilestone CreateMilestone(int salesOrderLineId, string milestoneType, DateTimeOffset targetDate, DateTimeOffset now)
    {
        return new ScheduleMilestone
        {
            SalesOrderLineId = salesOrderLineId,
            MilestoneType = milestoneType,
            TargetDate = targetDate,
            ActualDate = null,
            Notes = now > targetDate ? "At Risk" : null,
        };
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
