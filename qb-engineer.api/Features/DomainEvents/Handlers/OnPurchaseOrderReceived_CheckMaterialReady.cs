using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.DomainEvents.Handlers;

public class OnPurchaseOrderReceived_CheckMaterialReady(
    AppDbContext db,
    IClock clock,
    ILogger<OnPurchaseOrderReceived_CheckMaterialReady> logger)
    : INotificationHandler<PurchaseOrderReceivedEvent>
{
    public async Task Handle(PurchaseOrderReceivedEvent notification, CancellationToken ct)
    {
        var po = await db.PurchaseOrders
            .Include(p => p.Lines)
            .Include(p => p.Job)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == notification.PurchaseOrderId, ct);

        if (po?.JobId is null) return;

        // Check if all PO lines for this job have been fully received
        var allPosForJob = await db.PurchaseOrders
            .Include(p => p.Lines)
            .Where(p => p.JobId == po.JobId)
            .AsNoTracking()
            .ToListAsync(ct);

        var allFullyReceived = allPosForJob
            .SelectMany(p => p.Lines)
            .All(line => line.ReceivedQuantity >= line.OrderedQuantity);

        if (!allFullyReceived) return;

        // Check if a materials-ready follow-up already exists
        var existingFollowUp = await db.FollowUpTasks
            .AnyAsync(f =>
                f.SourceEntityType == "Job" &&
                f.SourceEntityId == po.JobId &&
                f.TriggerType == FollowUpTriggerType.MaterialsReady &&
                f.Status == FollowUpStatus.Open, ct);

        if (existingFollowUp) return;

        // Assign to the job's assignee, or a manager
        var assigneeId = po.Job?.AssigneeId ?? 0;
        if (assigneeId == 0)
        {
            assigneeId = await db.UserRoles
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .Where(x => x.Name == "Manager" || x.Name == "Admin")
                .Select(x => x.UserId)
                .FirstOrDefaultAsync(ct);
        }

        if (assigneeId == 0) return;

        var jobNumber = po.Job?.JobNumber ?? po.JobId.ToString();

        db.FollowUpTasks.Add(new FollowUpTask
        {
            Title = $"Materials ready for JOB-{jobNumber}",
            Description = $"All purchase orders for Job {jobNumber} have been fully received. Production can begin.",
            AssignedToUserId = assigneeId,
            DueDate = clock.UtcNow.AddDays(1),
            SourceEntityType = "Job",
            SourceEntityId = po.JobId.Value,
            TriggerType = FollowUpTriggerType.MaterialsReady,
            Status = FollowUpStatus.Open,
        });

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Materials ready for Job {JobId} — all POs received", po.JobId);
    }
}
