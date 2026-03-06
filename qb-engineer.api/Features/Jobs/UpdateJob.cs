using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record UpdateJobCommand(
    int Id,
    string? Title,
    string? Description,
    int? AssigneeId,
    int? CustomerId,
    JobPriority? Priority,
    DateTime? DueDate) : IRequest<JobDetailDto>;

public class UpdateJobHandler(AppDbContext db, IMediator mediator) : IRequestHandler<UpdateJobCommand, JobDetailDto>
{
    public async Task<JobDetailDto> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await db.Jobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken)
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

        await db.SaveChangesAsync(cancellationToken);

        return await mediator.Send(new GetJobByIdQuery(job.Id), cancellationToken);
    }
}
