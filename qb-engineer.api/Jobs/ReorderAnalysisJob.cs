using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Jobs;

/// <summary>
/// Daily Hangfire job — analyzes raw material burn rates, creates reorder suggestions
/// for parts that fall below their reorder thresholds, and notifies purchasing users.
/// </summary>
public class ReorderAnalysisJob(
    AppDbContext db,
    ILogger<ReorderAnalysisJob> logger)
{
    private static readonly BinMovementReason[] ConsumptionReasons =
        [BinMovementReason.Pick, BinMovementReason.Ship];

    private static readonly PurchaseOrderStatus[] OpenPoStatuses =
    [
        PurchaseOrderStatus.Draft,
        PurchaseOrderStatus.Submitted,
        PurchaseOrderStatus.Acknowledged,
        PurchaseOrderStatus.PartiallyReceived,
    ];

    public async Task RunAnalysisAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var cutoff90 = now.AddDays(-90);

        logger.LogInformation("[ReorderAnalysis] Starting daily reorder analysis at {Time}", now);

        // Load all active parts with replenishment settings
        var parts = await db.Parts
            .Where(p => p.DeletedAt == null)
            .ToListAsync();

        if (parts.Count == 0)
        {
            logger.LogInformation("[ReorderAnalysis] No parts found — skipping");
            return;
        }

        var partIds = parts.Select(p => p.Id).ToList();

        // Current stock per part
        var stockByPart = await db.BinContents
            .Where(bc => bc.EntityType == "part"
                && partIds.Contains(bc.EntityId)
                && bc.RemovedAt == null)
            .GroupBy(bc => bc.EntityId)
            .Select(g => new
            {
                PartId = g.Key,
                OnHand = g.Sum(bc => bc.Quantity),
                Reserved = g.Sum(bc => bc.ReservedQuantity),
            })
            .ToListAsync();

        var stockMap = stockByPart.ToDictionary(s => s.PartId);

        // Consumption movements over last 90 days
        var movements = await db.BinMovements
            .Where(m => m.EntityType == "part"
                && partIds.Contains(m.EntityId)
                && m.Reason != null
                && ConsumptionReasons.Contains(m.Reason!.Value)
                && m.MovedAt >= cutoff90)
            .Select(m => new { m.EntityId, m.Quantity, m.MovedAt })
            .ToListAsync();

        // Incoming PO quantities per part
        var incomingRaw = await db.PurchaseOrderLines
            .Include(l => l.PurchaseOrder)
            .Where(l => partIds.Contains(l.PartId)
                && l.PurchaseOrder.DeletedAt == null
                && OpenPoStatuses.Contains(l.PurchaseOrder.Status))
            .Select(l => new
            {
                l.PartId,
                RemainingQty = (decimal)(l.OrderedQuantity - l.ReceivedQuantity),
                l.PurchaseOrder.ExpectedDeliveryDate,
            })
            .ToListAsync();

        var incomingMap = incomingRaw
            .GroupBy(x => x.PartId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalQty = g.Sum(x => x.RemainingQty),
                    EarliestDate = g.Min(x => x.ExpectedDeliveryDate),
                });

        // Existing pending suggestions — don't create duplicates
        var existingPendingPartIds = await db.ReorderSuggestions
            .Where(s => s.Status == ReorderSuggestionStatus.Pending)
            .Select(s => s.PartId)
            .ToHashSetAsync();

        // Expire suggestions where stock has recovered
        var pendingSuggestions = await db.ReorderSuggestions
            .Where(s => s.Status == ReorderSuggestionStatus.Pending)
            .ToListAsync();

        var expiredCount = 0;
        foreach (var pending in pendingSuggestions)
        {
            var stock = stockMap.TryGetValue(pending.PartId, out var s) ? s : null;
            var available = (stock?.OnHand ?? 0m) - (stock?.Reserved ?? 0m);
            var incoming = incomingMap.TryGetValue(pending.PartId, out var inc) ? inc : null;
            var incomingQty = incoming?.TotalQty ?? 0m;

            var part = parts.FirstOrDefault(p => p.Id == pending.PartId);
            if (part == null) continue;

            if (!NeedsReorder(part, available, incomingQty, pending.BurnRateDailyAvg))
            {
                pending.Status = ReorderSuggestionStatus.Expired;
                expiredCount++;
            }
        }

        if (expiredCount > 0)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("[ReorderAnalysis] Expired {Count} resolved suggestion(s)", expiredCount);
        }

        // Analyze each part for new suggestions
        var newSuggestions = new List<ReorderSuggestion>();

        foreach (var part in parts)
        {
            if (existingPendingPartIds.Contains(part.Id))
                continue;

            var stock = stockMap.TryGetValue(part.Id, out var s) ? s : null;
            var onHand = stock?.OnHand ?? 0m;
            var reserved = stock?.Reserved ?? 0m;
            var available = onHand - reserved;

            var incoming = incomingMap.TryGetValue(part.Id, out var inc) ? inc : null;
            var incomingQty = incoming?.TotalQty ?? 0m;
            var earliestArrival = incoming?.EarliestDate;

            var partMovements = movements.Where(m => m.EntityId == part.Id).ToList();

            var burnRate30 = CalcBurnRate(partMovements.Select(m => (m.Quantity, m.MovedAt)).ToList(), now, 30);
            var burnRate60 = CalcBurnRate(partMovements.Select(m => (m.Quantity, m.MovedAt)).ToList(), now, 60);
            var burnRate90 = CalcBurnRate(partMovements.Select(m => (m.Quantity, m.MovedAt)).ToList(), now, 90);

            var bestBurnRate = burnRate90 ?? burnRate60 ?? burnRate30 ?? 0m;
            var windowDays = burnRate90.HasValue ? 90 : burnRate60.HasValue ? 60 : burnRate30.HasValue ? 30 : 0;

            if (!NeedsReorder(part, available, incomingQty, bestBurnRate))
                continue;

            int? daysRemaining = null;
            DateTimeOffset? projectedStockout = null;

            if (bestBurnRate > 0)
            {
                var effectiveStock = available + incomingQty;
                var days = (double)(effectiveStock / bestBurnRate);
                daysRemaining = (int)Math.Floor(days);
                projectedStockout = now.AddDays(days);
            }

            // Suggested quantity: cover lead time + safety stock + reorder qty preference
            decimal suggestedQty;
            if (part.ReorderQuantity.HasValue && part.ReorderQuantity.Value > 0)
            {
                suggestedQty = part.ReorderQuantity.Value;
            }
            else if (bestBurnRate > 0)
            {
                var coverDays = (part.LeadTimeDays ?? 14) + (part.SafetyStockDays ?? 14);
                suggestedQty = bestBurnRate * coverDays;
            }
            else
            {
                // No burn rate — suggest minimum threshold amount
                suggestedQty = part.MinStockThreshold ?? 10m;
            }

            newSuggestions.Add(new ReorderSuggestion
            {
                PartId = part.Id,
                VendorId = part.PreferredVendorId,
                CurrentStock = onHand,
                AvailableStock = available,
                BurnRateDailyAvg = bestBurnRate,
                BurnRateWindowDays = windowDays,
                DaysOfStockRemaining = daysRemaining,
                ProjectedStockoutDate = projectedStockout,
                IncomingPoQuantity = incomingQty,
                EarliestPoArrival = earliestArrival,
                SuggestedQuantity = Math.Ceiling(suggestedQty),
            });
        }

        if (newSuggestions.Count == 0)
        {
            logger.LogInformation("[ReorderAnalysis] No new reorder suggestions needed");
            return;
        }

        db.ReorderSuggestions.AddRange(newSuggestions);
        await db.SaveChangesAsync();

        logger.LogInformation(
            "[ReorderAnalysis] Created {Count} new reorder suggestion(s)", newSuggestions.Count);

        // Notify purchasing users (Admin + Manager roles)
        var notifyUserIds = await db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == "Admin" || x.Name == "Manager")
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in notifyUserIds)
        {
            db.Set<Notification>().Add(new Notification
            {
                Type = "reorder_suggestions_ready",
                Severity = "warning",
                Source = "inventory",
                Title = "Reorder Suggestions Ready",
                Message = $"{newSuggestions.Count} part(s) need replenishment. Review and approve purchase orders.",
                EntityType = "reorder_suggestions",
                UserId = userId,
            });
        }

        await db.SaveChangesAsync();
        logger.LogInformation(
            "[ReorderAnalysis] Notified {Count} user(s)", notifyUserIds.Count);
    }

    private static decimal? CalcBurnRate(
        List<(decimal Quantity, DateTimeOffset MovedAt)> movements, DateTimeOffset now, int windowDays)
    {
        var cutoff = now.AddDays(-windowDays);
        var total = movements
            .Where(m => m.MovedAt >= cutoff)
            .Sum(m => m.Quantity);

        return total > 0 ? total / windowDays : null;
    }

    private static bool NeedsReorder(
        Part part, decimal available, decimal incomingQty, decimal burnRate)
    {
        if (part.ReorderPoint.HasValue)
            return (available + incomingQty) <= part.ReorderPoint.Value;

        if (burnRate > 0)
        {
            var coverDays = (part.LeadTimeDays ?? 14) + (part.SafetyStockDays ?? 7);
            return (available + incomingQty) < burnRate * coverDays;
        }

        if (part.MinStockThreshold.HasValue)
            return available <= part.MinStockThreshold.Value;

        return false;
    }
}
