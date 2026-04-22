using MediatR;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// RFC 7592 client self-deletion. The client authenticates with its registration_access_token
/// and permanently revokes itself. All active authorizations and tokens are invalidated.
/// The OpenIddict application record is retained for audit traceability; the sidecar metadata
/// transitions to <see cref="OidcClientStatus.Revoked"/>.
/// </summary>
public record DeleteSelfClientCommand(
    string ClientId,
    string RawRegistrationAccessToken,
    string? CallerIp) : IRequest<Unit>;

public class DeleteSelfClientHandler(
    AppDbContext db,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager,
    IOpenIddictAuthorizationManager authManager,
    IOpenIddictTokenManager tokenManager)
    : IRequestHandler<DeleteSelfClientCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSelfClientCommand request, CancellationToken ct)
    {
        var metadata = await OidcSelfAuth.AuthenticateAsync(db, request.ClientId, request.RawRegistrationAccessToken, ct);

        metadata.Status = OidcClientStatus.Revoked;
        metadata.RegistrationAccessTokenHash = null;
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
            OidcAuditEventType.ClientSelfDeleted,
            actorIp: request.CallerIp,
            clientId: request.ClientId,
            ct: ct);

        return Unit.Value;
    }
}
