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
    DateTime? ClockedInAt);

public record GetClockStatusQuery : IRequest<List<ClockWorkerModel>>;

public class GetClockStatusHandler(AppDbContext db)
    : IRequestHandler<GetClockStatusQuery, List<ClockWorkerModel>>
{
    public async Task<List<ClockWorkerModel>> Handle(GetClockStatusQuery request, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => u.IsActive)
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), u.Initials, u.AvatarColor })
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;
        var latestEvents = await db.ClockEvents
            .Where(e => e.Timestamp >= today)
            .GroupBy(e => e.UserId)
            .Select(g => g.OrderByDescending(e => e.Timestamp).First())
            .ToListAsync(ct);

        var eventMap = latestEvents.ToDictionary(e => e.UserId);

        return users.Select(u =>
        {
            var hasEvent = eventMap.TryGetValue(u.Id, out var evt);
            var isClockedIn = hasEvent && evt!.EventType == ClockEventType.ClockIn;
            return new ClockWorkerModel(
                u.Id,
                u.Name,
                u.Initials ?? "??",
                u.AvatarColor ?? "#94a3b8",
                isClockedIn,
                isClockedIn ? evt!.Timestamp : null);
        }).OrderBy(w => w.Name).ToList();
    }
}
