using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Services;
using QBEngineer.Api.Workflows;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Parts.PromoteStatus;

/// <summary>
/// Workflow Pattern Phase 3 — Authoritative Part status promotion gate
/// (Draft → Active in v1; future transitions plug into the same handler).
/// Runs the entity readiness validators server-side; failures return 409
/// with the missing-validators envelope so callers can render
/// "Missing: BOM, Routing" with jump-to links.
///
/// The workflow Mark-Complete button delegates to this same handler — no
/// duplicate completion logic.
/// </summary>
public record PromotePartStatusCommand(int PartId, PromoteEntityStatusRequestModel Body)
    : IRequest<PartDetailResponseModel>;

public class PromotePartStatusValidator : AbstractValidator<PromotePartStatusCommand>
{
    public PromotePartStatusValidator()
    {
        RuleFor(x => x.Body.TargetStatus)
            .NotEmpty()
            .Must(s => s is "Active" or "Obsolete" or "Prototype")
            .WithMessage("targetStatus must be Active, Obsolete, or Prototype.");
    }
}

public class PromotePartStatusHandler(
    AppDbContext db,
    IPartRepository repo,
    IEntityReadinessService readiness,
    IEnumerable<IWorkflowEntityPromoter> promoters,
    ISystemAuditWriter auditWriter,
    IClock clock) : IRequestHandler<PromotePartStatusCommand, PartDetailResponseModel>
{
    private readonly Dictionary<string, IWorkflowEntityPromoter> _promoters =
        promoters.ToDictionary(p => p.EntityType, StringComparer.OrdinalIgnoreCase);

    public async Task<PartDetailResponseModel> Handle(PromotePartStatusCommand request, CancellationToken ct)
    {
        const string EntityType = "Part";
        // Run readiness gate.
        var missing = await readiness.GetMissingValidatorsAsync(EntityType, request.PartId, ct);
        if (missing.Count > 0)
        {
            var payload = missing.Select(m => new MissingValidatorResponseModel(
                m.ValidatorId, m.DisplayNameKey, m.MissingMessageKey)).ToList();
            throw new WorkflowMissingValidatorsException(
                payload,
                $"Cannot promote Part {request.PartId} to {request.Body.TargetStatus} — readiness validators not satisfied.");
        }

        if (!_promoters.TryGetValue(EntityType, out var promoter))
            throw new InvalidOperationException(
                $"No workflow entity promoter registered for entity type '{EntityType}'.");

        await promoter.PromoteAsync(request.PartId, request.Body.TargetStatus, ct);

        // Best-effort: if a workflow run is in flight against this part and the
        // promotion landed us on the canonical "Active" target, mark the run
        // complete in the same operation so callers see one fewer race.
        var run = await db.WorkflowRuns
            .FirstOrDefaultAsync(r => r.EntityType == EntityType
                                      && r.EntityId == request.PartId
                                      && r.CompletedAt == null
                                      && r.AbandonedAt == null, ct);
        if (run is not null)
        {
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
                    targetStatus = request.Body.TargetStatus,
                }),
                ct: ct);
        }

        await auditWriter.WriteAsync(
            action: WorkflowAuditEvents.EntityStatusPromoted,
            userId: db.CurrentUserId ?? 0,
            entityType: EntityType,
            entityId: request.PartId,
            details: JsonSerializer.Serialize(new
            {
                entityType = EntityType,
                entityId = request.PartId,
                targetStatus = request.Body.TargetStatus,
                workflowRunId = run?.Id,
            }),
            ct: ct);

        return (await repo.GetDetailAsync(request.PartId, ct))
            ?? throw new KeyNotFoundException($"Part id {request.PartId} not found after promotion.");
    }
}
