using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ShopFloor;

public record ClockWorkerModel(
    int UserId,
    string Name,
    string Initials,
    string AvatarColor,
    bool IsClockedIn,
    DateTime? ClockedInAt,
    string Status,
    string? CurrentTask,
    string? CurrentJobNumber,
    string TimeOnTask);

public record GetClockStatusQuery(int? TeamId = null) : IRequest<List<ClockWorkerModel>>;

public class GetClockStatusHandler(AppDbContext db)
    : IRequestHandler<GetClockStatusQuery, List<ClockWorkerModel>>
{
    public async Task<List<ClockWorkerModel>> Handle(GetClockStatusQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var usersQuery = db.Users.Where(u => u.IsActive);
        if (request.TeamId.HasValue)
            usersQuery = usersQuery.Where(u => u.TeamId == request.TeamId.Value);

        var users = await usersQuery
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), u.Initials, u.AvatarColor })
            .ToListAsync(ct);

        var latestEvents = await db.ClockEvents
            .Where(e => e.Timestamp >= today)
            .GroupBy(e => e.UserId)
            .Select(g => g.OrderByDescending(e => e.Timestamp).First())
            .ToListAsync(ct);

        var eventMap = latestEvents.ToDictionary(e => e.UserId);

        // Get active timers for all users
        var activeTimers = await db.TimeEntries
            .Include(t => t.Job)
            .Where(t => t.TimerStart != null && t.TimerStop == null)
            .ToListAsync(ct);

        var timersByUser = activeTimers.ToDictionary(t => t.UserId);

        return users.Select(u =>
        {
            var hasEvent = eventMap.TryGetValue(u.Id, out var evt);
            var isClockedIn = hasEvent && evt!.EventType is ClockEventType.ClockIn or ClockEventType.BreakEnd;
            var isOnBreak = hasEvent && evt!.EventType == ClockEventType.BreakStart;

            string status;
            if (isOnBreak) status = "OnBreak";
            else if (isClockedIn) status = "In";
            else status = "Out";

            timersByUser.TryGetValue(u.Id, out var timer);
            var clockInTime = isClockedIn && hasEvent ? evt!.Timestamp : (DateTime?)null;

            var timeOnTask = "";
            if (isClockedIn && timer?.TimerStart != null)
                timeOnTask = FormatDuration(now - timer.TimerStart.Value);
            else if (isClockedIn && clockInTime.HasValue)
                timeOnTask = FormatDuration(now - clockInTime.Value);
            else if (isOnBreak && hasEvent)
                timeOnTask = FormatDuration(now - evt!.Timestamp);

            return new ClockWorkerModel(
                u.Id,
                u.Name,
                u.Initials ?? "??",
                u.AvatarColor ?? "#94a3b8",
                isClockedIn || isOnBreak,
                clockInTime,
                status,
                timer?.Job?.Title,
                timer?.Job?.JobNumber,
                timeOnTask);
        }).OrderBy(w => w.Status == "Out").ThenBy(w => w.Name).ToList();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1) return "< 1m";
        if (duration.TotalHours < 1) return $"{(int)duration.TotalMinutes}m";
        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }
}
