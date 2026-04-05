using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.ShopFloor;

public record ClockWorkerModel(
    int UserId,
    string Name,
    string Email,
    string Initials,
    string AvatarColor,
    bool IsClockedIn,
    DateTimeOffset? ClockedInAt,
    string Status,
    string? CurrentTask,
    string? CurrentJobNumber,
    string TimeOnTask,
    DateTimeOffset? StatusSince,
    List<WorkerAssignmentModel> Assignments);

public record GetClockStatusQuery(int? TeamId = null) : IRequest<List<ClockWorkerModel>>;

public class GetClockStatusHandler(AppDbContext db)
    : IRequestHandler<GetClockStatusQuery, List<ClockWorkerModel>>
{
    public async Task<List<ClockWorkerModel>> Handle(GetClockStatusQuery request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var today = now.Date;

        var usersQuery = db.Users.Where(u => u.IsActive);
        if (request.TeamId.HasValue)
            usersQuery = usersQuery.Where(u => u.TeamId == request.TeamId.Value);

        var users = await usersQuery
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), u.Email, u.Initials, u.AvatarColor })
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

        // Load assigned shop-floor jobs per user (production/maintenance only, not R&D/admin)
        var shopFloorTrackIds = await db.TrackTypes
            .Where(t => t.IsShopFloor && t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToList();
        var assignedJobs = await db.Jobs
            .Include(j => j.CurrentStage)
            .Where(j => j.AssigneeId.HasValue
                && userIds.Contains(j.AssigneeId.Value)
                && shopFloorTrackIds.Contains(j.TrackTypeId)
                && !j.IsArchived
                && j.CompletedDate == null)
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.DueDate ?? DateTimeOffset.MaxValue)
            .Select(j => new
            {
                j.Id,
                j.AssigneeId,
                j.JobNumber,
                j.Title,
                Priority = j.Priority.ToString(),
                StageName = j.CurrentStage.Name,
                StageColor = j.CurrentStage.Color ?? "#94a3b8",
                j.DueDate,
            })
            .ToListAsync(ct);

        var assignmentsByUser = assignedJobs
            .GroupBy(j => j.AssigneeId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(j => new WorkerAssignmentModel(
                    j.Id,
                    j.JobNumber,
                    j.Title,
                    j.Priority,
                    j.StageName,
                    j.StageColor,
                    j.DueDate.HasValue && j.DueDate.Value.Date < today,
                    false)).ToList());

        // Mark active timer jobs
        foreach (var timer in activeTimers)
        {
            if (timer.JobId.HasValue && assignmentsByUser.TryGetValue(timer.UserId, out var list))
            {
                var idx = list.FindIndex(a => a.JobId == timer.JobId.Value);
                if (idx >= 0)
                    list[idx] = list[idx] with { HasActiveTimer = true };
            }
        }

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
            var clockInTime = isClockedIn && hasEvent ? evt!.Timestamp : (DateTimeOffset?)null;

            DateTimeOffset? statusSince = null;
            var timeOnTask = "";
            if (isClockedIn && timer?.TimerStart != null)
            {
                statusSince = timer.TimerStart.Value;
                timeOnTask = FormatDuration(now - statusSince.Value);
            }
            else if (isClockedIn && clockInTime.HasValue)
            {
                statusSince = clockInTime.Value;
                timeOnTask = FormatDuration(now - statusSince.Value);
            }
            else if (isOnBreak && hasEvent)
            {
                statusSince = evt!.Timestamp;
                timeOnTask = FormatDuration(now - statusSince.Value);
            }

            assignmentsByUser.TryGetValue(u.Id, out var userAssignments);

            return new ClockWorkerModel(
                u.Id,
                u.Name,
                u.Email ?? "",
                u.Initials ?? "??",
                u.AvatarColor ?? "#94a3b8",
                isClockedIn || isOnBreak,
                clockInTime,
                status,
                timer?.Job?.Title,
                timer?.Job?.JobNumber,
                timeOnTask,
                statusSince,
                userAssignments ?? []);
        }).OrderBy(w => w.Status == "Out").ThenBy(w => w.Name).ToList();
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours}h {duration.Minutes:D2}m {duration.Seconds:D2}s";
        if (duration.TotalMinutes >= 1)
            return $"{(int)duration.TotalMinutes}m {duration.Seconds:D2}s";
        return $"{duration.Seconds}s";
    }
}
