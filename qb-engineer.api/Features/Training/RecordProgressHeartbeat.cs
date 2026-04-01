using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

public record RecordProgressHeartbeatCommand(int UserId, int ModuleId, int Seconds) : IRequest;

public class RecordProgressHeartbeatHandler(AppDbContext db) : IRequestHandler<RecordProgressHeartbeatCommand>
{
    public async Task Handle(RecordProgressHeartbeatCommand request, CancellationToken ct)
    {
        var progress = await db.TrainingProgress
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.ModuleId == request.ModuleId, ct);

        if (progress is null)
        {
            db.TrainingProgress.Add(new TrainingProgress
            {
                UserId = request.UserId,
                ModuleId = request.ModuleId,
                Status = TrainingProgressStatus.InProgress,
                StartedAt = DateTimeOffset.UtcNow,
                TimeSpentSeconds = Math.Max(0, request.Seconds),
            });
        }
        else if (progress.Status != TrainingProgressStatus.Completed)
        {
            progress.TimeSpentSeconds += Math.Max(0, request.Seconds);
        }

        await db.SaveChangesAsync(ct);
    }
}
