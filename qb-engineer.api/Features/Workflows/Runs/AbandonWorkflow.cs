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
/// Workflow Pattern Phase 3 — Abandon the run. Marks the run abandoned and
/// soft-deletes the entity if it is still in <c>status='Draft'</c>. If the
/// entity has already been promoted (or the user opened a workflow against
/// an existing entity), the run is abandoned but the entity stays.
/// </summary>
public record AbandonWorkflowCommand(int RunId, AbandonWorkflowRequestModel Body)
    : IRequest<WorkflowRunResponseModel>;

public class AbandonWorkflowHandler(
    AppDbContext db,
    IEnumerable<IWorkflowFieldApplier> appliers,
    ISystemAuditWriter auditWriter,
    IClock clock) : IRequestHandler<AbandonWorkflowCommand, WorkflowRunResponseModel>
{
    private readonly Dictionary<string, IWorkflowFieldApplier> _appliers =
        appliers.ToDictionary(a => a.EntityType, StringComparer.OrdinalIgnoreCase);

    public async Task<WorkflowRunResponseModel> Handle(AbandonWorkflowCommand request, CancellationToken ct)
    {
        var run = await db.WorkflowRuns.FirstOrDefaultAsync(r => r.Id == request.RunId, ct)
            ?? throw new KeyNotFoundException($"Workflow run id {request.RunId} not found.");
        if (run.AbandonedAt is not null) return run.ToResponse(); // idempotent
        if (run.CompletedAt is not null)
            throw new InvalidOperationException("Cannot abandon a completed run.");

        // Soft-delete the entity if still Draft.
        if (_appliers.TryGetValue(run.EntityType, out var applier))
        {
            await applier.SoftDeleteIfDraftAsync(run.EntityId, ct);
        }

        run.AbandonedAt = clock.UtcNow;
        run.AbandonedReason = string.IsNullOrWhiteSpace(request.Body.Reason) ? "user" : request.Body.Reason;
        if (run.AbandonedReason!.Length > 64) run.AbandonedReason = run.AbandonedReason[..64];
        run.LastActivityAt = clock.UtcNow;

        await db.SaveChangesAsync(ct);

        await auditWriter.WriteAsync(
            action: WorkflowAuditEvents.Abandoned,
            userId: db.CurrentUserId ?? 0,
            entityType: WorkflowAuditEvents.EntityType,
            entityId: run.Id,
            details: JsonSerializer.Serialize(new
            {
                runId = run.Id,
                reason = run.AbandonedReason,
                entityType = run.EntityType,
                entityId = run.EntityId,
            }),
            ct: ct);

        return run.ToResponse();
    }
}
