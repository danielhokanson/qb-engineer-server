using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Jobs;

/// <summary>
/// Daily Hangfire job — detects orphaned clock-ins (no matching clock-out) from the
/// previous day and notifies both the employee and their manager.
/// </summary>
public class CheckMismatchedClockEventsJob(
    AppDbContext db,
    ILogger<CheckMismatchedClockEventsJob> logger)
{
    public async Task CheckMismatchedEventsAsync()
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var dayStart = yesterday.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = yesterday.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var events = await db.ClockEvents
            .Where(e => e.Timestamp >= dayStart && e.Timestamp <= dayEnd)
            .OrderBy(e => e.Timestamp)
            .ToListAsync();

        if (events.Count == 0)
        {
            logger.LogInformation("[MismatchedClockJob] No clock events found for {Date}", yesterday);
            return;
        }

        // Group by user, check if their last event of the day is a ClockIn (no matching ClockOut)
        var mismatchedUserIds = events
            .GroupBy(e => e.UserId)
            .Where(g =>
            {
                var last = g.OrderByDescending(e => e.Timestamp).First();
                return last.EventType == ClockEventType.ClockIn;
            })
            .Select(g => g.Key)
            .ToList();

        if (mismatchedUserIds.Count == 0)
        {
            logger.LogInformation("[MismatchedClockJob] All clock events matched for {Date}", yesterday);
            return;
        }

        logger.LogWarning(
            "[MismatchedClockJob] {Count} user(s) with unmatched clock-in on {Date}",
            mismatchedUserIds.Count, yesterday);

        // Check for duplicate notifications (avoid re-notifying for same date)
        var existingNotifications = await db.Notifications
            .Where(n => n.Type == "mismatched_clock_event"
                && mismatchedUserIds.Contains(n.UserId)
                && n.CreatedAt >= dayStart && n.CreatedAt <= DateTimeOffset.UtcNow)
            .Select(n => n.UserId)
            .ToListAsync();

        var newUserIds = mismatchedUserIds.Except(existingNotifications).ToList();
        if (newUserIds.Count == 0)
        {
            logger.LogInformation("[MismatchedClockJob] All mismatched users already notified");
            return;
        }

        // Get manager user IDs
        var managerUserIds = await db.UserRoles
            .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .Where(x => x.Name == "Admin" || x.Name == "Manager")
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        foreach (var userId in newUserIds)
        {
            var employee = await db.Users.FindAsync(userId);
            if (employee is null) continue;

            var employeeName = $"{employee.LastName}, {employee.FirstName}";

            // Notify the employee
            db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = "mismatched_clock_event",
                Severity = "warning",
                Source = "time_tracking",
                Title = "Unmatched Clock-In",
                Message = $"You have an unmatched clock-in from {yesterday:MM/dd/yyyy}. Please contact your manager to correct your timesheet.",
                EntityType = "clock_events",
            });

            // Notify managers
            foreach (var managerId in managerUserIds)
            {
                if (managerId == userId) continue;

                db.Notifications.Add(new Notification
                {
                    UserId = managerId,
                    Type = "mismatched_clock_event",
                    Severity = "info",
                    Source = "time_tracking",
                    Title = "Employee Unmatched Clock-In",
                    Message = $"{employeeName} has an unmatched clock-in from {yesterday:MM/dd/yyyy}.",
                    EntityType = "clock_events",
                });
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation(
            "[MismatchedClockJob] Sent notifications for {Count} user(s) with unmatched clock events",
            newUserIds.Count);
    }
}
