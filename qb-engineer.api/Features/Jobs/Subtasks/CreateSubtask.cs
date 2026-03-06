using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record CreateSubtaskCommand(
    int JobId,
    string Text,
    int? AssigneeId) : IRequest<SubtaskDto>;

public class CreateSubtaskHandler(AppDbContext db) : IRequestHandler<CreateSubtaskCommand, SubtaskDto>
{
    public async Task<SubtaskDto> Handle(CreateSubtaskCommand request, CancellationToken cancellationToken)
    {
        var jobExists = await db.Jobs.AnyAsync(j => j.Id == request.JobId, cancellationToken);
        if (!jobExists)
            throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var maxSortOrder = await db.JobSubtasks
            .Where(s => s.JobId == request.JobId)
            .MaxAsync(s => (int?)s.SortOrder, cancellationToken) ?? 0;

        var subtask = new JobSubtask
        {
            JobId = request.JobId,
            Text = request.Text,
            AssigneeId = request.AssigneeId,
            SortOrder = maxSortOrder + 1,
        };

        db.JobSubtasks.Add(subtask);
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
