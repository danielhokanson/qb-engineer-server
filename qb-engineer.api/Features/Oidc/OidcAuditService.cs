using System.Text.Json;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public class OidcAuditService(AppDbContext db, IClock clock) : IOidcAuditService
{
    public async Task RecordAsync(
        OidcAuditEventType eventType,
        int? actorUserId = null,
        string? actorIp = null,
        string? clientId = null,
        int? ticketId = null,
        string? scopeName = null,
        object? details = null,
        CancellationToken ct = default)
    {
        db.OidcAuditEvents.Add(new OidcAuditEvent
        {
            EventType = eventType,
            ActorUserId = actorUserId,
            ActorIpAddress = actorIp,
            ClientId = clientId,
            TicketId = ticketId,
            ScopeName = scopeName,
            DetailsJson = details is null ? null : JsonSerializer.Serialize(details),
            CreatedAt = clock.UtcNow,
        });

        await db.SaveChangesAsync(ct);
    }
}
