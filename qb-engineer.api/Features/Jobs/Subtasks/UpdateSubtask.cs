using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record UpdateSubtaskCommand(
    int JobId,
    int SubtaskId,
    string? Text,
    bool? IsCompleted,
    int? AssigneeId) : IRequest<SubtaskDto>;

public class UpdateSubtaskHandler(AppDbContext db) : IRequestHandler<UpdateSubtaskCommand, SubtaskDto>
{
    public async Task<SubtaskDto> Handle(UpdateSubtaskCommand request, CancellationToken cancellationToken)
    {
        var subtask = await db.JobSubtasks
            .FirstOrDefaultAsync(s => s.Id == request.SubtaskId && s.JobId == request.JobId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Subtask {request.SubtaskId} not found for Job {request.JobId}.");

        if (request.Text is not null)
            subtask.Text = request.Text;

        if (request.AssigneeId.HasValue)
            subtask.AssigneeId = request.AssigneeId.Value;

        if (request.IsCompleted.HasValue)
        {
            subtask.IsCompleted = request.IsCompleted.Value;

            if (request.IsCompleted.Value)
            {
                subtask.CompletedAt = DateTime.UtcNow;
                // CompletedById would typically come from the current user context;
                // left unset here since there's no ICurrentUser service wired yet.
            }
            else
            {
                subtask.CompletedAt = null;
                subtask.CompletedById = null;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SubtaskDto(
            subtask.Id,
            subtask.JobId,
            subtask.Text,
            subtask.IsCompleted,
            subtask.AssigneeId,
            subtask.SortOrder,
            subtask.CompletedAt);
    }
}
