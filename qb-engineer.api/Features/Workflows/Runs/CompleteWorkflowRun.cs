using System.Text.Json;

using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Services;
using QBEngineer.Api.Workflows;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Workflows.Runs;

/// <summary>
/// Workflow Pattern Phase 3 — Mark Complete. This is sugar for "promote
/// entity status" per the design doc — the same readiness gate runs here as
/// when the user clicks Promote on the entity detail page directly.
///
/// Phase 3 wires Part → Active. Other entity types follow the same shape
/// once their promoters are registered.
/// </summary>
public record CompleteWorkflowRunCommand(int RunId) : IRequest<WorkflowRunResponseModel>;

public class CompleteWorkflowRunHandler(
    AppDbContext db,
    IEntityReadinessService readiness,
    IEnumerable<IWorkflowEntityPromoter> promoters,
    ISystemAuditWriter auditWriter,
    IClock clock) : IRequestHandler<CompleteWorkflowRunCommand, WorkflowRunResponseModel>
{
    private readonly Dictionary<string, IWorkflowEntityPromoter> _promoters =
        promoters.ToDictionary(p => p.EntityType, StringComparer.OrdinalIgnoreCase);

    public async Task<WorkflowRunResponseModel> Handle(CompleteWorkflowRunCommand request, CancellationToken ct)
    {
        var run = await db.WorkflowRuns.FirstOrDefaultAsync(r => r.Id == request.RunId, ct)
            ?? throw new KeyNotFoundException($"Workflow run id {request.RunId} not found.");
        if (run.CompletedAt is not null) return run.ToResponse(); // idempotent
        if (run.AbandonedAt is not null)
            throw new InvalidOperationException("Cannot complete an abandoned run.");

        var missing = await readiness.GetMissingValidatorsAsync(run.EntityType, run.EntityId, ct);
        if (missing.Count > 0)
        {
            var payload = missing.Select(m => new MissingValidatorResponseModel(
                m.ValidatorId, m.DisplayNameKey, m.MissingMessageKey)).ToList();
            throw new WorkflowMissingValidatorsException(
                payload,
                $"Cannot complete workflow {run.Id} — readiness validators not satisfied.");
        }

        if (!_promoters.TryGetValue(run.EntityType, out var promoter))
            throw new InvalidOperationException(
                $"No workflow entity promoter registered for entity type '{run.EntityType}'.");

        // v1: promote to "Active" (the only canonical target). Future variants
        // can carry a target_status column on workflow_runs if needed.
        await promoter.PromoteAsync(run.EntityId, "Active", ct);

        run.CompletedAt = clock.UtcNow;
        run.LastActivityAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditWriter.WriteAsync(
            action: WorkflowAuditEvents.Completed,
            userId: db.CurrentUserId ?? 0,
            entityType: WorkflowAuditEvents.EntityType,
            entityId: run.Id,
            details: JsonSerializer.Serialize(new
            {
                runId = run.Id,
                entityType = run.EntityType,
                entityId = run.EntityId,
                targetStatus = "Active",
            }),
            ct: ct);

        return run.ToResponse();
    }
}
