using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Services;
using QBEngineer.Api.Workflows;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Workflows.Runs;

/// <summary>
/// Workflow Pattern Phase 3 — Save the current step's fields and (if its
/// completion gates pass) advance <c>current_step_id</c> to the next step.
/// Idempotent on the field-application side; advancing twice from the same
/// step is a no-op once the pointer has moved.
/// </summary>
public record PatchWorkflowStepCommand(int RunId, PatchWorkflowStepRequestModel Body)
    : IRequest<WorkflowRunResponseModel>;

public class PatchWorkflowStepValidator : AbstractValidator<PatchWorkflowStepCommand>
{
    public PatchWorkflowStepValidator()
    {
        RuleFor(x => x.Body.StepId).NotEmpty().MaximumLength(64);
    }
}

public class PatchWorkflowStepHandler(
    AppDbContext db,
    IEnumerable<IWorkflowFieldApplier> appliers,
    IEntityReadinessService readiness,
    ISystemAuditWriter auditWriter,
    IClock clock) : IRequestHandler<PatchWorkflowStepCommand, WorkflowRunResponseModel>
{
    private readonly Dictionary<string, IWorkflowFieldApplier> _appliers =
        appliers.ToDictionary(a => a.EntityType, StringComparer.OrdinalIgnoreCase);

    public async Task<WorkflowRunResponseModel> Handle(PatchWorkflowStepCommand request, CancellationToken ct)
    {
        var run = await db.WorkflowRuns.FirstOrDefaultAsync(r => r.Id == request.RunId, ct)
            ?? throw new KeyNotFoundException($"Workflow run id {request.RunId} not found.");
        if (run.CompletedAt is not null || run.AbandonedAt is not null)
            throw new InvalidOperationException("Workflow run is no longer active.");

        var def = await db.WorkflowDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DefinitionId == run.DefinitionId, ct)
            ?? throw new InvalidOperationException(
                $"Workflow definition '{run.DefinitionId}' missing — run is orphaned.");

        var steps = WorkflowStepHelper.ParseSteps(def.StepsJson);
        var stepIndex = WorkflowStepHelper.IndexOf(steps, request.Body.StepId);
        if (stepIndex < 0)
            throw new InvalidOperationException(
                $"Step '{request.Body.StepId}' is not part of definition '{def.DefinitionId}'.");
        var step = steps[stepIndex];

        // Apply the field payload via the entity-type adapter.
        if (!_appliers.TryGetValue(run.EntityType, out var applier))
            throw new InvalidOperationException(
                $"No workflow field applier registered for entity type '{run.EntityType}'.");
        await applier.ApplyAsync(run.EntityId, request.Body.Fields, ct);

        // Re-evaluate the step's gates. If all required gates pass, advance.
        var advanced = false;
        if (step.CompletionGates.Count == 0 || await GatesPassAsync(run.EntityType, run.EntityId, step.CompletionGates, ct))
        {
            // Move pointer forward IF we're at the current step. If the user
            // re-edited an earlier completed step, leave current_step_id where
            // it is (D2: clicking back to an earlier step doesn't reset).
            if (string.Equals(run.CurrentStepId, step.Id, StringComparison.OrdinalIgnoreCase))
            {
                run.CurrentStepId = stepIndex + 1 < steps.Count ? steps[stepIndex + 1].Id : run.CurrentStepId;
                advanced = true;
            }
        }

        run.LastActivityAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditWriter.WriteAsync(
            action: WorkflowAuditEvents.StepAdvanced,
            userId: db.CurrentUserId ?? 0,
            entityType: WorkflowAuditEvents.EntityType,
            entityId: run.Id,
            details: JsonSerializer.Serialize(new
            {
                runId = run.Id,
                stepId = step.Id,
                advanced,
                currentStepId = run.CurrentStepId,
            }),
            ct: ct);

        return run.ToResponse();
    }

    private async Task<bool> GatesPassAsync(
        string entityType, int entityId,
        IReadOnlyList<string> gateIds, CancellationToken ct)
    {
        var missing = await readiness.GetMissingValidatorsAsync(entityType, entityId, ct);
        if (missing.Count == 0) return true;
        // gates pass if NONE of the gateIds are in missing.ValidatorId set.
        var failing = missing.Select(m => m.ValidatorId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var gate in gateIds)
            if (failing.Contains(gate)) return false;
        return true;
    }
}
