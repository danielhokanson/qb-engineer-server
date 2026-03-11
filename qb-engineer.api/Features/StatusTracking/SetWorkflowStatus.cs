using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.StatusTracking;

public record SetWorkflowStatusCommand(
    string EntityType,
    int EntityId,
    SetStatusRequestModel Data) : IRequest<StatusEntryResponseModel>;

public class SetWorkflowStatusCommandValidator : AbstractValidator<SetWorkflowStatusCommand>
{
    public SetWorkflowStatusCommandValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EntityId).GreaterThan(0);
        RuleFor(x => x.Data.StatusCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Data.Notes).MaximumLength(2000).When(x => x.Data.Notes is not null);
    }
}

public class SetWorkflowStatusHandler(
    AppDbContext db,
    IStatusEntryRepository repository)
    : IRequestHandler<SetWorkflowStatusCommand, StatusEntryResponseModel>
{
    public async Task<StatusEntryResponseModel> Handle(
        SetWorkflowStatusCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Close any existing active workflow status
        var currentWorkflow = await db.StatusEntries
            .Where(s => s.EntityType == request.EntityType
                        && s.EntityId == request.EntityId
                        && s.Category == "workflow"
                        && s.EndedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var entry in currentWorkflow)
        {
            entry.EndedAt = now;
        }

        // Resolve label from reference_data (fallback to StatusCode if not found)
        var label = await db.ReferenceData
            .Where(r => r.Code == request.Data.StatusCode && r.IsActive)
            .Select(r => r.Label)
            .FirstOrDefaultAsync(cancellationToken) ?? request.Data.StatusCode;

        var statusEntry = new StatusEntry
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            StatusCode = request.Data.StatusCode,
            StatusLabel = label,
            Category = "workflow",
            StartedAt = now,
            EndedAt = null,
            Notes = request.Data.Notes?.Trim(),
        };

        await db.StatusEntries.AddAsync(statusEntry, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Reload to return with SetBy info
        var history = await repository.GetHistoryAsync(request.EntityType, request.EntityId, cancellationToken);
        return history.First(h => h.Id == statusEntry.Id);
    }
}
