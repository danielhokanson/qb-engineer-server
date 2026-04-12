using MediatR;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Scheduling;

public record LockScheduledOperationCommand(int ScheduledOperationId, bool IsLocked) : IRequest;

public class LockScheduledOperationHandler(AppDbContext db) : IRequestHandler<LockScheduledOperationCommand>
{
    public async Task Handle(LockScheduledOperationCommand request, CancellationToken cancellationToken)
    {
        var op = await db.ScheduledOperations.FindAsync([request.ScheduledOperationId], cancellationToken)
            ?? throw new KeyNotFoundException($"Scheduled operation {request.ScheduledOperationId} not found.");

        op.IsLocked = request.IsLocked;
        await db.SaveChangesAsync(cancellationToken);
    }
}
