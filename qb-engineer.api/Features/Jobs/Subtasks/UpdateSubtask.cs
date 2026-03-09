using MediatR;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs.Subtasks;

public record UpdateSubtaskCommand(
    int JobId,
    int SubtaskId,
    string? Text,
    bool? IsCompleted,
    int? AssigneeId) : IRequest<SubtaskResponseModel>;

public class UpdateSubtaskHandler(
    ISubtaskRepository repo,
    IHubContext<BoardHub> boardHub) : IRequestHandler<UpdateSubtaskCommand, SubtaskResponseModel>
{
    public async Task<SubtaskResponseModel> Handle(UpdateSubtaskCommand request, CancellationToken cancellationToken)
    {
        var subtask = await repo.FindAsync(request.SubtaskId, request.JobId, cancellationToken)
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
            }
            else
            {
                subtask.CompletedAt = null;
                subtask.CompletedById = null;
            }
        }

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
            .SendAsync("subtaskChanged", new { jobId = request.JobId, subtask = result, changeType = "updated" }, cancellationToken);

        return result;
    }
}
