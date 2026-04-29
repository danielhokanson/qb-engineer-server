using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Services;
using QBEngineer.Api.Workflows;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Workflows.Runs;

/// <summary>
/// Workflow Pattern Phase 3 — Start a new workflow run. Server creates the
/// entity row in <c>status='Draft'</c> via the registered
/// <see cref="IWorkflowEntityCreator"/> for the requested entityType, then
/// the workflow_runs row pinned to the supplied <c>definitionId</c> (Q2
/// versioning rule).
/// </summary>
public record StartWorkflowRunCommand(StartWorkflowRunRequestModel Body)
    : IRequest<WorkflowRunResponseModel>;

public class StartWorkflowRunValidator : AbstractValidator<StartWorkflowRunCommand>
{
    public StartWorkflowRunValidator()
    {
        RuleFor(x => x.Body.EntityType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Body.DefinitionId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Body.Mode)
            .Must(m => m is null || m == "express" || m == "guided")
            .WithMessage("mode must be 'express' or 'guided' when supplied.");
    }
}

public class StartWorkflowRunHandler(
    AppDbContext db,
    IEnumerable<IWorkflowEntityCreator> creators,
    ISystemAuditWriter auditWriter,
    IClock clock) : IRequestHandler<StartWorkflowRunCommand, WorkflowRunResponseModel>
{
    private readonly Dictionary<string, IWorkflowEntityCreator> _creators =
        creators.ToDictionary(c => c.EntityType, StringComparer.OrdinalIgnoreCase);

    public async Task<WorkflowRunResponseModel> Handle(StartWorkflowRunCommand request, CancellationToken ct)
    {
        var body = request.Body;

        // Resolve definition (must exist and match entity type).
        var def = await db.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DefinitionId == body.DefinitionId, ct)
            ?? throw new KeyNotFoundException($"Workflow definition '{body.DefinitionId}' not found.");
        if (!string.Equals(def.EntityType, body.EntityType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Definition '{def.DefinitionId}' targets entity type '{def.EntityType}', not '{body.EntityType}'.");

        // Resolve creator for the entity type.
        if (!_creators.TryGetValue(body.EntityType, out var creator))
            throw new InvalidOperationException(
                $"No workflow entity creator registered for entity type '{body.EntityType}'.");

        // Create the draft entity. The adapter calls SaveChanges itself (single
        // PG round-trip; cross-entity transaction not required here because
        // the workflow_run row is independent of the entity row's existence
        // until both succeed).
        var entityId = await creator.CreateDraftAsync(body.InitialEntityData, ct);

        // Compute first step from the definition's StepsJson.
        var firstStepId = ReadFirstStepId(def.StepsJson);
        var mode = body.Mode ?? def.DefaultMode;
        var now = clock.UtcNow;

        var run = new WorkflowRun
        {
            EntityType = body.EntityType,
            EntityId = entityId,
            DefinitionId = body.DefinitionId,
            CurrentStepId = firstStepId,
            Mode = mode,
            StartedAt = now,
            StartedByUserId = db.CurrentUserId ?? 0,
            LastActivityAt = now,
        };
        db.WorkflowRuns.Add(run);

        // Junction row for the primary entity (Q3).
        db.WorkflowRunEntities.Add(new WorkflowRunEntity
        {
            Run = run,
            EntityType = body.EntityType,
            EntityId = entityId,
            Role = "primary",
        });

        await db.SaveChangesAsync(ct);

        await auditWriter.WriteAsync(
            action: WorkflowAuditEvents.Started,
            userId: db.CurrentUserId ?? 0,
            entityType: WorkflowAuditEvents.EntityType,
            entityId: run.Id,
            details: JsonSerializer.Serialize(new
            {
                runId = run.Id,
                entityType = run.EntityType,
                entityId = run.EntityId,
                definitionId = run.DefinitionId,
                mode = run.Mode,
                currentStepId = run.CurrentStepId,
            }),
            ct: ct);

        return run.ToResponse();
    }

    private static string? ReadFirstStepId(string stepsJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(stepsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;
            foreach (var step in doc.RootElement.EnumerateArray())
            {
                if (step.ValueKind == JsonValueKind.Object &&
                    step.TryGetProperty("id", out var id) &&
                    id.ValueKind == JsonValueKind.String)
                {
                    return id.GetString();
                }
            }
        }
        catch (JsonException) { }
        return null;
    }
}
