using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Temporarily disables an active client. Reversible — call <see cref="ApproveClientCommand"/>
/// to restore. Existing tokens for the client are not invalidated automatically; use
/// <see cref="RevokeClientCommand"/> for permanent removal + token invalidation.
/// </summary>
public record SuspendClientCommand(string ClientId, int ActorUserId, string? ActorIp, string? Reason)
    : IRequest<Unit>;

public class SuspendClientHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<SuspendClientCommand, Unit>
{
    public async Task<Unit> Handle(SuspendClientCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status is OidcClientStatus.Revoked or OidcClientStatus.Suspended)
        {
            return Unit.Value;
        }

        metadata.Status = OidcClientStatus.Suspended;
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ClientSuspended,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new { request.Reason },
            ct: ct);

        return Unit.Value;
    }
}
