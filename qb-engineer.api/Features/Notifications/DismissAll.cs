using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Notifications;

public record DismissAllCommand : IRequest;

public class DismissAllHandler(
    INotificationRepository repo,
    IHttpContextAccessor httpContext) : IRequestHandler<DismissAllCommand>
{
    public async Task Handle(DismissAllCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await repo.DismissAllAsync(userId, cancellationToken);
    }
}
