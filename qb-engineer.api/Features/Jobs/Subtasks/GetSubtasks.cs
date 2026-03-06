using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record GetSubtasksQuery(int JobId) : IRequest<List<SubtaskDto>>;

public class GetSubtasksHandler(AppDbContext db) : IRequestHandler<GetSubtasksQuery, List<SubtaskDto>>
{
    public async Task<List<SubtaskDto>> Handle(GetSubtasksQuery request, CancellationToken cancellationToken)
    {
        var result = await db.JobSubtasks
            .Where(s => s.JobId == request.JobId)
            .OrderBy(s => s.SortOrder)
            .Select(s => new SubtaskDto(
                s.Id,
                s.JobId,
                s.Text,
                s.IsCompleted,
                s.AssigneeId,
                s.SortOrder,
                s.CompletedAt))
            .ToListAsync(cancellationToken);

        return result;
    }
}
