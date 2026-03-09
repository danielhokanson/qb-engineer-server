using MediatR;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record MoveJobStageCommand(int JobId, int StageId) : IRequest<JobDetailResponseModel>;

public class MoveJobStageHandler(
    IJobRepository jobRepo,
    ITrackTypeRepository trackRepo,
    IActivityLogRepository actRepo,
    IMediator mediator,
    IHubContext<BoardHub> boardHub) : IRequestHandler<MoveJobStageCommand, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(MoveJobStageCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepo.FindAsync(request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var targetStage = await trackRepo.FindStageAsync(request.StageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Stage with ID {request.StageId} not found.");

        if (targetStage.TrackTypeId != job.TrackTypeId)
            throw new InvalidOperationException(
                $"Stage {request.StageId} does not belong to track type {job.TrackTypeId}.");

        var previousStage = await trackRepo.FindStageAsync(job.CurrentStageId, cancellationToken);
        var previousStageName = previousStage?.Name;
        var previousStageId = job.CurrentStageId;

        job.CurrentStageId = request.StageId;

        var maxPosition = await jobRepo.GetMaxBoardPositionAsync(request.StageId, cancellationToken);
        job.BoardPosition = maxPosition + 1;

        var log = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.StageMoved,
            FieldName = "CurrentStageId",
            OldValue = previousStageName,
            NewValue = targetStage.Name,
            Description = $"Moved from {previousStageName} to {targetStage.Name}.",
        };
        await actRepo.AddAsync(log, cancellationToken);

        await jobRepo.SaveChangesAsync(cancellationToken);

        var result = await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);

        // Broadcast to board group
        await boardHub.Clients.Group($"board:{job.TrackTypeId}")
            .SendAsync("jobMoved", new BoardJobMovedEvent(
                job.Id, previousStageId, request.StageId,
                targetStage.Name, job.BoardPosition), cancellationToken);

        return result;
    }
}
