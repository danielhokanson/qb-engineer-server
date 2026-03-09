using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record UpdateJobCommand(
    int Id,
    string? Title,
    string? Description,
    int? AssigneeId,
    int? CustomerId,
    JobPriority? Priority,
    DateTime? DueDate) : IRequest<JobDetailResponseModel>;

public class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
    }
}

public class UpdateJobHandler(
    IJobRepository repo,
    IMediator mediator,
    IHubContext<BoardHub> boardHub) : IRequestHandler<UpdateJobCommand, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found.");

        if (request.Title is not null)
            job.Title = request.Title;

        if (request.Description is not null)
            job.Description = request.Description;

        if (request.AssigneeId.HasValue)
            job.AssigneeId = request.AssigneeId.Value;

        if (request.CustomerId.HasValue)
            job.CustomerId = request.CustomerId.Value;

        if (request.Priority.HasValue)
            job.Priority = request.Priority.Value;

        if (request.DueDate.HasValue)
            job.DueDate = request.DueDate.Value;

        await repo.SaveChangesAsync(cancellationToken);

        var result = await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);

        // Broadcast to board + job detail subscribers
        var evt = new BoardJobUpdatedEvent(job.Id, result);
        await boardHub.Clients.Group($"board:{job.TrackTypeId}")
            .SendAsync("jobUpdated", evt, cancellationToken);
        await boardHub.Clients.Group($"job:{job.Id}")
            .SendAsync("jobUpdated", evt, cancellationToken);

        return result;
    }
}
