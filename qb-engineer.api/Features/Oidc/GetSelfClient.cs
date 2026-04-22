using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// RFC 7592 client self-read: returns the current registration for a client that authenticates
/// with its registration_access_token. The token is rotated on every call — the response
/// includes the new raw token exactly once, and the old hash is replaced before returning.
/// </summary>
public record GetSelfClientQuery(
    string ClientId,
    string RawRegistrationAccessToken,
    string? CallerIp) : IRequest<SelfClientResponse>;

public record SelfClientResponse(
    string ClientId,
    string? ClientName,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string> PostLogoutRedirectUris,
    IReadOnlyList<string> GrantTypes,
    IReadOnlyList<string> ResponseTypes,
    IReadOnlyList<string> Scopes,
    string TokenEndpointAuthMethod,
    string NewRegistrationAccessToken,
    OidcClientStatus Status,
    bool RequireConsent,
    bool IsFirstParty);

public class GetSelfClientHandler(
    AppDbContext db,
    IClock clock,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager)
    : IRequestHandler<GetSelfClientQuery, SelfClientResponse>
{
    public async Task<SelfClientResponse> Handle(GetSelfClientQuery request, CancellationToken ct)
    {
        var metadata = await OidcSelfAuth.AuthenticateAsync(db, request.ClientId, request.RawRegistrationAccessToken, ct);

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct)
            ?? throw new OidcRegistrationException(
                OidcRegistrationException.Errors.InvalidToken,
                "Client not found.");

        var displayName = await appManager.GetDisplayNameAsync(app, ct);
        var redirectUris = await appManager.GetRedirectUrisAsync(app, ct);
        var postLogoutUris = await appManager.GetPostLogoutRedirectUrisAsync(app, ct);
        var permissions = await appManager.GetPermissionsAsync(app, ct);

        var scopes = permissions
            .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope, StringComparison.Ordinal))
            .Select(p => p[OpenIddictConstants.Permissions.Prefixes.Scope.Length..])
            .ToArray();

        var newTokenRaw = OidcCrypto.GenerateRegistrationAccessToken();
        metadata.RegistrationAccessTokenHash = OidcCrypto.HashSha256(newTokenRaw);
        metadata.RegistrationAccessTokenRotatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ClientSelfRead,
            actorIp: request.CallerIp,
            clientId: request.ClientId,
            ct: ct);

        return new SelfClientResponse(
            request.ClientId,
            displayName,
            redirectUris.ToArray(),
            postLogoutUris.ToArray(),
            GrantTypes: new[] { "authorization_code", "refresh_token" },
            ResponseTypes: new[] { "code" },
            Scopes: scopes,
            TokenEndpointAuthMethod: "client_secret_basic",
            NewRegistrationAccessToken: newTokenRaw,
            Status: metadata.Status,
            RequireConsent: metadata.RequireConsent,
            IsFirstParty: metadata.IsFirstParty);
    }
}
