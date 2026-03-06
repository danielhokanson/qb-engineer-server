using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record MoveJobStageCommand(int JobId, int StageId) : IRequest<JobDetailDto>;

public class MoveJobStageHandler(AppDbContext db, IMediator mediator) : IRequestHandler<MoveJobStageCommand, JobDetailDto>
{
    public async Task<JobDetailDto> Handle(MoveJobStageCommand request, CancellationToken cancellationToken)
    {
        var job = await db.Jobs
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        // Validate the target stage belongs to the same track type
        var targetStage = await db.JobStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Stage with ID {request.StageId} not found.");

        if (targetStage.TrackTypeId != job.TrackTypeId)
            throw new InvalidOperationException(
                $"Stage {request.StageId} does not belong to track type {job.TrackTypeId}.");

        var previousStageId = job.CurrentStageId;
        var previousStageName = await db.JobStages
            .Where(s => s.Id == previousStageId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(cancellationToken);

        // Update stage and board position
        job.CurrentStageId = request.StageId;

        var maxPosition = await db.Jobs
            .Where(j => j.CurrentStageId == request.StageId && j.Id != job.Id)
            .MaxAsync(j => (int?)j.BoardPosition, cancellationToken) ?? 0;

        job.BoardPosition = maxPosition + 1;

        // Create activity log
        var log = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.StageMoved,
            FieldName = "CurrentStageId",
            OldValue = previousStageName,
            NewValue = targetStage.Name,
            Description = $"Moved from {previousStageName} to {targetStage.Name}.",
        };
        db.JobActivityLogs.Add(log);

        await db.SaveChangesAsync(cancellationToken);

        return await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);
    }
}
