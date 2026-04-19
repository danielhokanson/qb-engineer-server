using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.DomainEvents.Handlers;

public class OnJobStageChanged_CheckShipReady(
    AppDbContext db,
    IClock clock,
    ILogger<OnJobStageChanged_CheckShipReady> logger)
    : INotificationHandler<JobStageChangedEvent>
{
    public async Task Handle(JobStageChangedEvent notification, CancellationToken ct)
    {
        var job = await db.Jobs
            .Include(j => j.CurrentStage)
            .Include(j => j.SalesOrderLine)
                .ThenInclude(l => l!.SalesOrder)
            .FirstOrDefaultAsync(j => j.Id == notification.JobId, ct);

        if (job?.SalesOrderLineId is null) return;

        var soLine = job.SalesOrderLine!;

        // Check if ALL jobs for this SO line are in a completed stage
        var allJobsForLine = await db.Jobs
            .Include(j => j.CurrentStage)
            .Where(j => j.SalesOrderLineId == soLine.Id)
            .ToListAsync(ct);

        // A stage is considered "complete" if it's past QC (last stage or shipped-type stages)
        var toStage = await db.JobStages
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == notification.ToStageId, ct);

        if (toStage is null) return;

        // Check if the stage name indicates completion (QC passed, Shipped, etc.)
        var completionStageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Shipped", "Invoiced/Sent", "Payment Received", "Completed"
        };

        // Only proceed if the job just moved to a completion-type stage
        if (!completionStageNames.Contains(toStage.Name) &&
            !toStage.Name.Contains("Ship", StringComparison.OrdinalIgnoreCase))
            return;

        // Check if all sibling jobs for this line are at completion stages
        var allComplete = allJobsForLine.All(j =>
            completionStageNames.Contains(j.CurrentStage.Name) ||
            j.CurrentStage.Name.Contains("Ship", StringComparison.OrdinalIgnoreCase));

        if (!allComplete) return;

        // Check if a ship-ready follow-up already exists for this SO line
        var existingFollowUp = await db.FollowUpTasks
            .AnyAsync(f =>
                f.SourceEntityType == "SalesOrderLine" &&
                f.SourceEntityId == soLine.Id &&
                f.TriggerType == FollowUpTriggerType.ShipReady &&
                f.Status == FollowUpStatus.Open, ct);

        if (existingFollowUp) return;

        // Find a shipping-responsible user (Office Manager or Manager)
        var shippingUserId = await db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == "OfficeManager" || x.Name == "Manager" || x.Name == "Admin")
            .Select(x => x.UserId)
            .FirstOrDefaultAsync(ct);

        if (shippingUserId == 0) return;

        db.FollowUpTasks.Add(new FollowUpTask
        {
            Title = $"SO line ready to ship — SO-{soLine.SalesOrder.OrderNumber}",
            Description = $"All production jobs for line {soLine.LineNumber} are complete. Ready to create shipment.",
            AssignedToUserId = shippingUserId,
            DueDate = clock.UtcNow.AddDays(1),
            SourceEntityType = "SalesOrderLine",
            SourceEntityId = soLine.Id,
            TriggerType = FollowUpTriggerType.ShipReady,
            Status = FollowUpStatus.Open,
        });

        db.ActivityLogs.Add(new ActivityLog
        {
            EntityType = "SalesOrder",
            EntityId = soLine.SalesOrderId,
            UserId = notification.UserId,
            Action = "ship_ready",
            Description = $"All jobs for SO line {soLine.LineNumber} are complete. Ready to ship.",
        });

        await db.SaveChangesAsync(ct);
        logger.LogInformation("SO line {LineId} for SO {OrderNumber} is ready to ship", soLine.Id, soLine.SalesOrder.OrderNumber);
    }
}
