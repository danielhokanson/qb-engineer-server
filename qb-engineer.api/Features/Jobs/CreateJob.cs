using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record CreateJobCommand(
    string Title,
    string? Description,
    int TrackTypeId,
    int? AssigneeId,
    int? CustomerId,
    JobPriority? Priority,
    DateTime? DueDate) : IRequest<JobDetailResponseModel>;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.TrackTypeId)
            .GreaterThan(0).WithMessage("TrackTypeId is required.");
    }
}

public class CreateJobHandler(
    IJobRepository jobRepo,
    ITrackTypeRepository trackRepo,
    IMediator mediator,
    IHubContext<BoardHub> boardHub,
    IBarcodeService barcodeService) : IRequestHandler<CreateJobCommand, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var firstStage = await trackRepo.FindFirstActiveStageAsync(request.TrackTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"No active stages found for TrackType {request.TrackTypeId}.");

        var jobNumber = await jobRepo.GenerateNextJobNumberAsync(cancellationToken);
        var maxPosition = await jobRepo.GetMaxBoardPositionAsync(firstStage.Id, cancellationToken);

        var job = new Job
        {
            JobNumber = jobNumber,
            Title = request.Title,
            Description = request.Description,
            TrackTypeId = request.TrackTypeId,
            CurrentStageId = firstStage.Id,
            AssigneeId = request.AssigneeId,
            CustomerId = request.CustomerId,
            Priority = request.Priority ?? JobPriority.Normal,
            DueDate = request.DueDate,
            BoardPosition = maxPosition + 1,
        };

        var log = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.Created,
            Description = $"Job {jobNumber} created.",
        };
        job.ActivityLogs.Add(log);

        await jobRepo.AddAsync(job, cancellationToken);
        await jobRepo.SaveChangesAsync(cancellationToken);

        await barcodeService.CreateBarcodeAsync(
            Core.Enums.BarcodeEntityType.Job, job.Id, job.JobNumber, cancellationToken);

        var result = await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);

        // Broadcast to board group
        await boardHub.Clients.Group($"board:{request.TrackTypeId}")
            .SendAsync("jobCreated", new BoardJobCreatedEvent(
                job.Id, job.JobNumber, job.Title, request.TrackTypeId,
                firstStage.Id, firstStage.Name, job.BoardPosition), cancellationToken);

        return result;
    }
}
