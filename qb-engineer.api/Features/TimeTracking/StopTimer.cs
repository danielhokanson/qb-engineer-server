using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using QBEngineer.Api.Hubs;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TimeTracking;

public record StopTimerCommand(StopTimerRequestModel Data) : IRequest<TimeEntryResponseModel>;

public class StopTimerHandler(
    ITimeTrackingRepository repo,
    IHttpContextAccessor httpContext,
    IHubContext<TimerHub> timerHub) : IRequestHandler<StopTimerCommand, TimeEntryResponseModel>
{
    public async Task<TimeEntryResponseModel> Handle(StopTimerCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var active = await repo.GetActiveTimerAsync(userId, cancellationToken);
        if (active is null)
            throw new InvalidOperationException("No active timer found.");

        var now = DateTime.UtcNow;
        active.TimerStop = now;
        active.DurationMinutes = (int)(now - active.TimerStart!.Value).TotalMinutes;
        if (!string.IsNullOrWhiteSpace(request.Data.Notes))
            active.Notes = request.Data.Notes.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        var result = (await repo.GetTimeEntryByIdAsync(active.Id, cancellationToken))!;

        // Broadcast to all tabs for this user
        await timerHub.Clients.Group($"user:{userId}")
            .SendAsync("timerStopped", new TimerStoppedEvent(userId, result), cancellationToken);

        return result;
    }
}
