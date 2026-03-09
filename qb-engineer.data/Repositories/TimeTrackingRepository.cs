using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class TimeTrackingRepository(AppDbContext db) : ITimeTrackingRepository
{
    public async Task<List<TimeEntryResponseModel>> GetTimeEntriesAsync(int? userId, int? jobId, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var query = db.TimeEntries.Include(t => t.Job).AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);

        if (jobId.HasValue)
            query = query.Where(t => t.JobId == jobId.Value);

        if (from.HasValue)
            query = query.Where(t => t.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(t => t.Date <= to.Value);

        var entries = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var userIds = entries.Select(t => t.UserId).Distinct().ToList();
        var users = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        return entries.Select(t => ToTimeEntryResponse(t, users)).ToList();
    }

    public async Task<TimeEntryResponseModel?> GetTimeEntryByIdAsync(int id, CancellationToken ct)
    {
        var entry = await db.TimeEntries.Include(t => t.Job)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (entry is null) return null;

        var users = await db.Users
            .Where(u => u.Id == entry.UserId)
            .ToDictionaryAsync(u => u.Id, ct);

        return ToTimeEntryResponse(entry, users);
    }

    public Task<TimeEntry?> FindTimeEntryAsync(int id, CancellationToken ct)
        => db.TimeEntries.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<TimeEntry?> GetActiveTimerAsync(int userId, CancellationToken ct)
        => db.TimeEntries.FirstOrDefaultAsync(t =>
            t.UserId == userId && t.TimerStart != null && t.TimerStop == null, ct);

    public async Task AddTimeEntryAsync(TimeEntry entry, CancellationToken ct)
    {
        await db.TimeEntries.AddAsync(entry, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<ClockEventResponseModel>> GetClockEventsAsync(int? userId, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var query = db.ClockEvents.AsQueryable();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);

        if (from.HasValue)
        {
            var fromDate = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(c => c.Timestamp >= fromDate);
        }

        if (to.HasValue)
        {
            var toDate = to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(c => c.Timestamp <= toDate);
        }

        var events = await query
            .OrderByDescending(c => c.Timestamp)
            .ToListAsync(ct);

        var userIds = events.Select(c => c.UserId).Distinct().ToList();
        var users = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        return events.Select(c =>
        {
            var userName = users.TryGetValue(c.UserId, out var user)
                ? $"{user.FirstName} {user.LastName}" : "Unknown";
            return new ClockEventResponseModel(
                c.Id, c.UserId, userName, c.EventType,
                c.Reason, c.ScanMethod, c.Timestamp, c.Source);
        }).ToList();
    }

    public Task<ClockEvent?> GetLastClockEventAsync(int userId, CancellationToken ct)
        => db.ClockEvents
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync(ct);

    public async Task AddClockEventAsync(ClockEvent clockEvent, CancellationToken ct)
    {
        await db.ClockEvents.AddAsync(clockEvent, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);

    private static TimeEntryResponseModel ToTimeEntryResponse(TimeEntry t, Dictionary<int, ApplicationUser> users)
    {
        var userName = users.TryGetValue(t.UserId, out var user)
            ? $"{user.FirstName} {user.LastName}" : "Unknown";
        return new TimeEntryResponseModel(
            t.Id, t.JobId, t.Job?.JobNumber,
            t.UserId, userName, t.Date, t.DurationMinutes,
            t.Category, t.Notes, t.TimerStart, t.TimerStop,
            t.IsManual, t.IsLocked, t.CreatedAt);
    }
}
