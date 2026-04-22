using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Admin query of the OIDC audit feed. Filters are all optional and AND'd together.
/// Returned newest-first, capped at <paramref name="Take"/> (max 500).
/// </summary>
public record ListAuditEventsQuery(
    OidcAuditEventType? EventType,
    string? ClientId,
    int? TicketId,
    int? ActorUserId,
    DateTimeOffset? Since,
    DateTimeOffset? Until,
    int Skip,
    int Take) : IRequest<IReadOnlyList<AuditEventListItem>>;

public record AuditEventListItem(
    int Id,
    OidcAuditEventType EventType,
    int? ActorUserId,
    string? ActorIpAddress,
    string? ClientId,
    int? TicketId,
    string? ScopeName,
    string? DetailsJson,
    DateTimeOffset CreatedAt);

public class ListAuditEventsHandler(AppDbContext db)
    : IRequestHandler<ListAuditEventsQuery, IReadOnlyList<AuditEventListItem>>
{
    public async Task<IReadOnlyList<AuditEventListItem>> Handle(ListAuditEventsQuery request, CancellationToken ct)
    {
        var take = Math.Clamp(request.Take <= 0 ? 100 : request.Take, 1, 500);
        var skip = Math.Max(0, request.Skip);

        var query = db.OidcAuditEvents.AsNoTracking().AsQueryable();
        if (request.EventType.HasValue) query = query.Where(e => e.EventType == request.EventType.Value);
        if (!string.IsNullOrWhiteSpace(request.ClientId)) query = query.Where(e => e.ClientId == request.ClientId);
        if (request.TicketId.HasValue) query = query.Where(e => e.TicketId == request.TicketId.Value);
        if (request.ActorUserId.HasValue) query = query.Where(e => e.ActorUserId == request.ActorUserId.Value);
        if (request.Since.HasValue) query = query.Where(e => e.CreatedAt >= request.Since.Value);
        if (request.Until.HasValue) query = query.Where(e => e.CreatedAt <= request.Until.Value);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(e => new AuditEventListItem(
                e.Id, e.EventType, e.ActorUserId, e.ActorIpAddress,
                e.ClientId, e.TicketId, e.ScopeName, e.DetailsJson, e.CreatedAt))
            .ToListAsync(ct);
    }
}
