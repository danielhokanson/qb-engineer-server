using MediatR;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record CreateSubtaskCommand(
    int JobId,
    string Text,
    int? AssigneeId) : IRequest<SubtaskResponseModel>;

public class CreateSubtaskHandler(
    ISubtaskRepository repo,
    IHubContext<BoardHub> boardHub) : IRequestHandler<CreateSubtaskCommand, SubtaskResponseModel>
{
    public async Task<SubtaskResponseModel> Handle(CreateSubtaskCommand request, CancellationToken cancellationToken)
    {
        var jobExists = await repo.JobExistsAsync(request.JobId, cancellationToken);
        if (!jobExists)
            throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var maxSortOrder = await repo.GetMaxSortOrderAsync(request.JobId, cancellationToken);

        var subtask = new JobSubtask
        {
            JobId = request.JobId,
            Text = request.Text,
            AssigneeId = request.AssigneeId,
            SortOrder = maxSortOrder + 1,
        };

        await repo.AddAsync(subtask, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);

        var result = new SubtaskResponseModel(
            subtask.Id,
            subtask.JobId,
            subtask.Text,
            subtask.IsCompleted,
            subtask.AssigneeId,
            subtask.SortOrder,
            subtask.CompletedAt);

        // Broadcast to job detail subscribers
        await boardHub.Clients.Group($"job:{request.JobId}")
            .SendAsync("subtaskChanged", new { jobId = request.JobId, subtask = result, changeType = "created" }, cancellationToken);

        return result;
    }
}
