using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Admin action to revoke an unredeemed registration ticket. Idempotent — safe to call
/// on already-redeemed/expired/revoked tickets; those return without side effects.
/// </summary>
public record RevokeTicketCommand(int TicketId, int ActorUserId, string? ActorIp, string? Reason)
    : IRequest<Unit>;

public class RevokeTicketHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<RevokeTicketCommand, Unit>
{
    public async Task<Unit> Handle(RevokeTicketCommand request, CancellationToken ct)
    {
        var ticket = await db.OidcRegistrationTickets.FirstOrDefaultAsync(
            t => t.Id == request.TicketId, ct)
            ?? throw new KeyNotFoundException($"Registration ticket {request.TicketId} not found.");

        if (ticket.Status != OidcTicketStatus.Issued)
        {
            return Unit.Value;
        }

        ticket.Status = OidcTicketStatus.Revoked;
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.TicketRevoked,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            ticketId: ticket.Id,
            details: new { request.Reason },
            ct: ct);

        return Unit.Value;
    }
}
