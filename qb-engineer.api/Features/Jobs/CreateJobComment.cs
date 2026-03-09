using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record CreateJobCommentCommand(int JobId, string Comment) : IRequest<ActivityResponseModel>;

public class CreateJobCommentHandler(
    IActivityLogRepository activityRepo,
    IHttpContextAccessor httpContext)
    : IRequestHandler<CreateJobCommentCommand, ActivityResponseModel>
{
    public async Task<ActivityResponseModel> Handle(
        CreateJobCommentCommand request, CancellationToken cancellationToken)
    {
        var jobExists = await activityRepo.JobExistsAsync(request.JobId, cancellationToken);
        if (!jobExists)
            throw new KeyNotFoundException($"Job with ID {request.JobId} not found.");

        var userId = int.Parse(
            httpContext.HttpContext!.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var log = new JobActivityLog
        {
            JobId = request.JobId,
            UserId = userId,
            Action = ActivityAction.CommentAdded,
            Description = request.Comment,
        };

        await activityRepo.AddAsync(log, cancellationToken);
        await activityRepo.SaveChangesAsync(cancellationToken);

        var activities = await activityRepo.GetByJobIdAsync(request.JobId, cancellationToken);
        return activities.First(a => a.Id == log.Id);
    }
}
