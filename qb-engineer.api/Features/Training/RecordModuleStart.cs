using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Training;

public record RecordModuleStartCommand(int UserId, int ModuleId) : IRequest;

public class RecordModuleStartHandler(AppDbContext db) : IRequestHandler<RecordModuleStartCommand>
{
    public async Task Handle(RecordModuleStartCommand request, CancellationToken ct)
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
                StartedAt = DateTime.UtcNow,
            });
        }
        else if (progress.Status != TrainingProgressStatus.Completed)
        {
            progress.Status = TrainingProgressStatus.InProgress;
            progress.StartedAt ??= DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
