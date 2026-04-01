using FluentValidation;
using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.StatusTracking;

public record ReleaseHoldCommand(
    int StatusEntryId,
    ReleaseHoldRequestModel? Data) : IRequest<StatusEntryResponseModel>;

public class ReleaseHoldHandler(IStatusEntryRepository repository)
    : IRequestHandler<ReleaseHoldCommand, StatusEntryResponseModel>
{
    public async Task<StatusEntryResponseModel> Handle(
        ReleaseHoldCommand request, CancellationToken cancellationToken)
    {
        var entry = await repository.FindAsync(request.StatusEntryId, cancellationToken)
            ?? throw new KeyNotFoundException($"StatusEntry with id {request.StatusEntryId} not found.");

        if (entry.Category != "hold" || entry.EndedAt is not null)
        {
            throw new InvalidOperationException("This status entry is not an active hold.");
        }

        entry.EndedAt = DateTimeOffset.UtcNow;

        if (request.Data?.Notes is not null)
        {
            entry.Notes = string.IsNullOrWhiteSpace(entry.Notes)
                ? request.Data.Notes.Trim()
                : $"{entry.Notes}\n---\nRelease: {request.Data.Notes.Trim()}";
        }

        await repository.SaveChangesAsync(cancellationToken);

        // Reload to return updated model
        var history = await repository.GetHistoryAsync(entry.EntityType, entry.EntityId, cancellationToken);
        return history.First(h => h.Id == entry.Id);
    }
}
