using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobByIdQuery(int Id) : IRequest<JobDetailDto>;

public class GetJobByIdHandler(AppDbContext db) : IRequestHandler<GetJobByIdQuery, JobDetailDto>
{
    public async Task<JobDetailDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var users = db.Users.AsQueryable();

        var result = await db.Jobs
            .Where(j => j.Id == request.Id)
            .Join(
                db.JobStages,
                j => j.CurrentStageId,
                s => s.Id,
                (j, s) => new { Job = j, Stage = s })
            .Join(
                db.TrackTypes,
                js => js.Job.TrackTypeId,
                t => t.Id,
                (js, t) => new { js.Job, js.Stage, TrackType = t })
            .GroupJoin(
                db.Customers,
                x => x.Job.CustomerId,
                c => c.Id,
                (x, customers) => new { x.Job, x.Stage, x.TrackType, Customers = customers })
            .SelectMany(
                x => x.Customers.DefaultIfEmpty(),
                (x, customer) => new { x.Job, x.Stage, x.TrackType, Customer = customer })
            .GroupJoin(
                users,
                x => x.Job.AssigneeId,
                u => u.Id,
                (x, assignees) => new { x.Job, x.Stage, x.TrackType, x.Customer, Assignees = assignees })
            .SelectMany(
                x => x.Assignees.DefaultIfEmpty(),
                (x, assignee) => new JobDetailDto(
                    x.Job.Id,
                    x.Job.JobNumber,
                    x.Job.Title,
                    x.Job.Description,
                    x.Job.TrackTypeId,
                    x.TrackType.Name,
                    x.Job.CurrentStageId,
                    x.Stage.Name,
                    x.Stage.Color,
                    x.Job.AssigneeId,
                    assignee != null ? assignee.Initials : null,
                    assignee != null ? assignee.FirstName + " " + assignee.LastName : null,
                    assignee != null ? assignee.AvatarColor : null,
                    x.Job.Priority.ToString(),
                    x.Job.CustomerId,
                    x.Customer != null ? x.Customer.Name : null,
                    x.Job.DueDate,
                    x.Job.StartDate,
                    x.Job.CompletedDate,
                    x.Job.IsArchived,
                    x.Job.BoardPosition,
                    x.Job.CreatedAt,
                    x.Job.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return result ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found.");
    }
}
