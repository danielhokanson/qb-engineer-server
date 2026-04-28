using System.Security.Claims;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Api.Features.DomainEvents;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record CreateJobCommand(
    string Title,
    string? Description,
    int TrackTypeId,
    int? AssigneeId,
    int? CustomerId,
    JobPriority? Priority,
    DateTimeOffset? DueDate,
    int? PartId = null) : IRequest<JobDetailResponseModel>;

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
    IBarcodeService barcodeService,
    IHttpContextAccessor httpContextAccessor,
    AppDbContext db) : IRequestHandler<CreateJobCommand, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        if (request.AssigneeId.HasValue)
            await AssigneeComplianceCheck.EnsureCanBeAssigned(db, request.AssigneeId.Value, cancellationToken);

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
            PartId = request.PartId,
        };

        // Phase 3 H4 / WU-20 — if this job is being released against a
        // part with a current BOM revision, pin that revision id so future
        // modifications to the BOM don't retroactively alter what this job
        // was built against. Captured at create time so the pin is in place
        // before the row is even saved (single SaveChanges).
        if (request.PartId is int pinPartId)
        {
            var currentRevId = await db.Parts
                .Where(p => p.Id == pinPartId)
                .Select(p => p.CurrentBomRevisionId)
                .FirstOrDefaultAsync(cancellationToken);
            job.BomRevisionIdAtRelease = currentRevId;
        }

        job.ActivityLogs.Add(new JobActivityLog
        {
            Action = ActivityAction.Created,
            Description = $"Job {jobNumber} created.",
        });

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

        // Publish domain event for calendar integration
        var userId = int.Parse(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        if (userId > 0)
            await mediator.Publish(new JobCreatedEvent(job.Id, userId), cancellationToken);

        return result;
    }
}
