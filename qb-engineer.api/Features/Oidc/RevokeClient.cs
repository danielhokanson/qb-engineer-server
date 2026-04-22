using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Permanent revocation. Marks the sidecar metadata as Revoked (soft) and invalidates all
/// active authorizations + tokens the client has issued. The OpenIddict application record
/// itself is retained for audit traceability.
/// </summary>
public record RevokeClientCommand(string ClientId, int ActorUserId, string? ActorIp, string? Reason)
    : IRequest<Unit>;

public class RevokeClientHandler(
    AppDbContext db,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager,
    IOpenIddictAuthorizationManager authManager,
    IOpenIddictTokenManager tokenManager)
    : IRequestHandler<RevokeClientCommand, Unit>
{
    public async Task<Unit> Handle(RevokeClientCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status == OidcClientStatus.Revoked)
        {
            return Unit.Value;
        }

        metadata.Status = OidcClientStatus.Revoked;
        await db.SaveChangesAsync(ct);

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct);
        if (app is not null)
        {
            var appId = await appManager.GetIdAsync(app, ct);
            if (!string.IsNullOrEmpty(appId))
            {
                await foreach (var auth in authManager.FindByApplicationIdAsync(appId, ct))
                {
                    await authManager.TryRevokeAsync(auth, ct);
                }
                await foreach (var token in tokenManager.FindByApplicationIdAsync(appId, ct))
                {
                    await tokenManager.TryRevokeAsync(token, ct);
                }
            }
        }

        await audit.RecordAsync(
            OidcAuditEventType.ClientRevoked,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new { request.Reason },
            ct: ct);

        return Unit.Value;
    }
}
