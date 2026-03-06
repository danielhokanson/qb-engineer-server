using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record CreateJobCommand(
    string Title,
    string? Description,
    int TrackTypeId,
    int? AssigneeId,
    int? CustomerId,
    JobPriority? Priority,
    DateTime? DueDate) : IRequest<JobDetailDto>;

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

public class CreateJobHandler(AppDbContext db, IMediator mediator) : IRequestHandler<CreateJobCommand, JobDetailDto>
{
    public async Task<JobDetailDto> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        // Determine the first stage for this track type
        var firstStage = await db.JobStages
            .Where(s => s.TrackTypeId == request.TrackTypeId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"No active stages found for TrackType {request.TrackTypeId}.");

        // Generate job number: find the max existing number, parse, increment
        var jobNumber = await GenerateJobNumberAsync(cancellationToken);

        // Determine board position (max + 1 in the target stage)
        var maxPosition = await db.Jobs
            .Where(j => j.CurrentStageId == firstStage.Id)
            .MaxAsync(j => (int?)j.BoardPosition, cancellationToken) ?? 0;

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

        db.Jobs.Add(job);

        // Create activity log entry
        var log = new JobActivityLog
        {
            JobId = job.Id,
            Action = ActivityAction.Created,
            Description = $"Job {jobNumber} created.",
        };
        // EF will resolve the temporary key for JobId after SaveChanges
        job.ActivityLogs.Add(log);

        await db.SaveChangesAsync(cancellationToken);

        // Return the full detail DTO via the existing GetJobById handler
        return await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);
    }

    private async Task<string> GenerateJobNumberAsync(CancellationToken cancellationToken)
    {
        var maxJobNumber = await db.Jobs
            .Select(j => j.JobNumber)
            .OrderByDescending(jn => jn)
            .FirstOrDefaultAsync(cancellationToken);

        if (maxJobNumber is not null && maxJobNumber.StartsWith("J-")
            && int.TryParse(maxJobNumber[2..], out var currentNumber))
        {
            return $"J-{currentNumber + 1}";
        }

        return "J-1001";
    }
}
