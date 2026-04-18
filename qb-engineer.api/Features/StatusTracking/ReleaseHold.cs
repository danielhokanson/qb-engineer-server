using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.StatusTracking;

public record ReleaseHoldCommand(
    int StatusEntryId,
    ReleaseHoldRequestModel? Data) : IRequest<StatusEntryResponseModel>;

public class ReleaseHoldHandler(
    IStatusEntryRepository repository,
    IActivityLogRepository activityRepo,
    IHttpContextAccessor httpContext)
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

        // Create activity log entry for hold release
        var userIdClaim = httpContext.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        int? currentUserId = userIdClaim is not null ? int.Parse(userIdClaim.Value) : null;

        var description = $"Hold released: {entry.StatusLabel}.";

        if (string.Equals(entry.EntityType, "job", System.StringComparison.OrdinalIgnoreCase))
        {
            await activityRepo.AddAsync(new JobActivityLog
            {
                JobId = entry.EntityId,
                UserId = currentUserId,
                Action = ActivityAction.StatusChanged,
                FieldName = "Hold",
                OldValue = entry.StatusLabel,
                NewValue = null,
                Description = description,
            }, cancellationToken);
        }
        else
        {
            await activityRepo.AddAsync(new ActivityLog
            {
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                UserId = currentUserId,
                Action = ActivityAction.StatusChanged.ToString(),
                FieldName = "Hold",
                OldValue = entry.StatusLabel,
                NewValue = null,
                Description = description,
            }, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);

        // Reload to return updated model
        var history = await repository.GetHistoryAsync(entry.EntityType, entry.EntityId, cancellationToken);
        return history.First(h => h.Id == entry.Id);
    }
}
