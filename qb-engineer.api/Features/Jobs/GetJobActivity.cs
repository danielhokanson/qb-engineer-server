using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobActivityQuery(int JobId) : IRequest<List<ActivityDto>>;

public class GetJobActivityHandler(AppDbContext db) : IRequestHandler<GetJobActivityQuery, List<ActivityDto>>
{
    public async Task<List<ActivityDto>> Handle(GetJobActivityQuery request, CancellationToken cancellationToken)
    {
        // Verify job exists
        var jobExists = await db.Jobs.AnyAsync(j => j.Id == request.JobId, cancellationToken);
        if (!jobExists)
            throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var users = db.Users.AsQueryable();

        var result = await db.JobActivityLogs
            .Where(l => l.JobId == request.JobId)
            .GroupJoin(
                users,
                l => l.UserId,
                u => u.Id,
                (l, assignees) => new { Log = l, Users = assignees })
            .SelectMany(
                x => x.Users.DefaultIfEmpty(),
                (x, user) => new ActivityDto(
                    x.Log.Id,
                    x.Log.Action.ToString(),
                    x.Log.FieldName,
                    x.Log.OldValue,
                    x.Log.NewValue,
                    x.Log.Description,
                    user != null ? user.Initials : null,
                    user != null ? user.FirstName + " " + user.LastName : null,
                    x.Log.CreatedAt))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return result;
    }
}
