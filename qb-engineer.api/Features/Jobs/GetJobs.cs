using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobsQuery(
    int? TrackTypeId,
    int? CurrentStageId,
    int? AssigneeId,
    bool IsArchived = false,
    string? Search = null) : IRequest<List<JobListDto>>;

public class GetJobsHandler(AppDbContext db) : IRequestHandler<GetJobsQuery, List<JobListDto>>
{
    public async Task<List<JobListDto>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Jobs.AsQueryable();

        query = query.Where(j => j.IsArchived == request.IsArchived);

        if (request.TrackTypeId.HasValue)
            query = query.Where(j => j.TrackTypeId == request.TrackTypeId.Value);

        if (request.CurrentStageId.HasValue)
            query = query.Where(j => j.CurrentStageId == request.CurrentStageId.Value);

        if (request.AssigneeId.HasValue)
            query = query.Where(j => j.AssigneeId == request.AssigneeId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(search) ||
                j.JobNumber.ToLower().Contains(search));
        }

        var users = db.Users.AsQueryable();

        var result = await query
            .Join(
                db.JobStages,
                j => j.CurrentStageId,
                s => s.Id,
                (j, s) => new { Job = j, Stage = s })
            .GroupJoin(
                db.Customers,
                js => js.Job.CustomerId,
                c => c.Id,
                (js, customers) => new { js.Job, js.Stage, Customers = customers })
            .SelectMany(
                x => x.Customers.DefaultIfEmpty(),
                (x, customer) => new { x.Job, x.Stage, Customer = customer })
            .GroupJoin(
                users,
                x => x.Job.AssigneeId,
                u => u.Id,
                (x, assignees) => new { x.Job, x.Stage, x.Customer, Assignees = assignees })
            .SelectMany(
                x => x.Assignees.DefaultIfEmpty(),
                (x, assignee) => new JobListDto(
                    x.Job.Id,
                    x.Job.JobNumber,
                    x.Job.Title,
                    x.Stage.Name,
                    x.Stage.Color,
                    assignee != null ? assignee.Initials : null,
                    assignee != null ? assignee.AvatarColor : null,
                    x.Job.Priority.ToString(),
                    x.Job.DueDate,
                    x.Job.DueDate.HasValue && x.Job.DueDate.Value < DateTime.UtcNow && x.Job.CompletedDate == null,
                    x.Customer != null ? x.Customer.Name : null))
            .OrderBy(dto => dto.Id)
            .ToListAsync(cancellationToken);

        return result;
    }
}
