using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Replaces the client's <c>client_secret</c> with a freshly generated value. The new raw secret
/// is returned exactly once. No grace period — the previous secret stops working immediately.
/// </summary>
public record RotateSecretCommand(string ClientId, int ActorUserId, string? ActorIp)
    : IRequest<RotateSecretResponse>;

public record RotateSecretResponse(string ClientId, string NewClientSecret, DateTimeOffset RotatedAt);

public class RotateSecretHandler(
    AppDbContext db,
    IClock clock,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager)
    : IRequestHandler<RotateSecretCommand, RotateSecretResponse>
{
    public async Task<RotateSecretResponse> Handle(RotateSecretCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status == OidcClientStatus.Revoked)
        {
            throw new InvalidOperationException("Cannot rotate secret on a revoked client.");
        }

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OpenIddict application '{request.ClientId}' not found.");

        var newSecret = OidcCrypto.GenerateClientSecret();
        var descriptor = new OpenIddictApplicationDescriptor();
        await appManager.PopulateAsync(descriptor, app, ct);
        descriptor.ClientSecret = newSecret;
        await appManager.UpdateAsync(app, descriptor, ct);

        var now = clock.UtcNow;
        metadata.LastSecretRotatedAt = now;
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.SecretRotated,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            ct: ct);

        return new RotateSecretResponse(request.ClientId, newSecret, now);
    }
}
