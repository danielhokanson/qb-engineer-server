using MediatR;
using Microsoft.AspNetCore.SignalR;

using QBEngineer.Api.Hubs;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Notifications;

public record CreateNotificationCommand(CreateNotificationRequestModel Data) : IRequest<NotificationResponseModel>;

public class CreateNotificationHandler(
    INotificationRepository repo,
    IHubContext<NotificationHub> notificationHub) : IRequestHandler<CreateNotificationCommand, NotificationResponseModel>
{
    public async Task<NotificationResponseModel> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var notification = new Notification
        {
            UserId = data.UserId,
            Type = data.Type,
            Severity = data.Severity,
            Source = data.Source,
            Title = data.Title,
            Message = data.Message,
            EntityType = data.EntityType,
            EntityId = data.EntityId,
            SenderId = data.SenderId,
        };

        await repo.AddAsync(notification, cancellationToken);

        var result = (await repo.GetByUserIdAsync(data.UserId, cancellationToken))
            .First(n => n.Id == notification.Id);

        // Push to user via SignalR
        await notificationHub.Clients.Group($"user:{data.UserId}")
            .SendAsync("notificationReceived", result, cancellationToken);

        return result;
    }
}
