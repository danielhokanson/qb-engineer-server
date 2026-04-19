using MediatR;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.FollowUpTasks;

public record DismissFollowUpTaskCommand(int Id, int UserId) : IRequest;

public class DismissFollowUpTaskHandler(
    AppDbContext db,
    IClock clock) : IRequestHandler<DismissFollowUpTaskCommand>
{
    public async Task Handle(DismissFollowUpTaskCommand request, CancellationToken ct)
    {
        var task = await db.FollowUpTasks.FindAsync([request.Id], ct)
            ?? throw new KeyNotFoundException($"FollowUpTask {request.Id} not found");

        if (task.AssignedToUserId != request.UserId)
            throw new UnauthorizedAccessException("Cannot dismiss a task assigned to another user");

        task.Status = FollowUpStatus.Dismissed;
        task.DismissedAt = clock.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
