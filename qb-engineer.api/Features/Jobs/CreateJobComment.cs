using System.Text.RegularExpressions;

using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Api.Features.Notifications;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record CreateJobCommentCommand(int JobId, string Comment) : IRequest<ActivityResponseModel>;

public class CreateJobCommentHandler(
    IActivityLogRepository activityRepo,
    IUserRepository userRepo,
    ISender sender,
    IHttpContextAccessor httpContext)
    : IRequestHandler<CreateJobCommentCommand, ActivityResponseModel>
{
    private static readonly Regex MentionPattern = new(@"@(\w+)", RegexOptions.Compiled);

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

        // Parse @mentions and create notifications
        var mentionedNames = MentionPattern.Matches(request.Comment)
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (mentionedNames.Count > 0)
        {
            var mentionedUsers = await userRepo.FindByNamesAsync(mentionedNames, cancellationToken);

            foreach (var user in mentionedUsers.Where(u => u.Id != userId))
            {
                await sender.Send(new CreateNotificationCommand(new CreateNotificationRequestModel(
                    UserId: user.Id,
                    Type: "mention",
                    Severity: "info",
                    Source: "user",
                    Title: "You were mentioned in a comment",
                    Message: request.Comment.Length > 200
                        ? request.Comment[..200] + "..."
                        : request.Comment,
                    EntityType: "Job",
                    EntityId: request.JobId,
                    SenderId: userId)), cancellationToken);
            }
        }

        var activities = await activityRepo.GetByJobIdAsync(request.JobId, cancellationToken);
        return activities.First(a => a.Id == log.Id);
    }
}
