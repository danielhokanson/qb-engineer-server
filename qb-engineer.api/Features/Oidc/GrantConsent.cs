using System.Collections.Immutable;
using System.Security.Claims;

using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Records a user's consent for a (user, client, scopes) tuple. Creates a permanent OpenIddict
/// authorization so subsequent <c>/connect/authorize</c> requests with the same or narrower scopes
/// do not prompt again. The Angular consent screen dispatches this command when the user clicks Allow.
/// </summary>
public record GrantConsentCommand(
    int UserId,
    string ClientId,
    IReadOnlyCollection<string> Scopes,
    string? ActorIp) : IRequest<Unit>;

public class GrantConsentHandler(
    AppDbContext db,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager,
    IOpenIddictAuthorizationManager authManager)
    : IRequestHandler<GrantConsentCommand, Unit>
{
    public async Task<Unit> Handle(GrantConsentCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status != OidcClientStatus.Active)
        {
            throw new InvalidOperationException("This client is not currently allowed to sign users in.");
        }

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OpenIddict application '{request.ClientId}' not found.");
        var appId = await appManager.GetIdAsync(app, ct)
            ?? throw new InvalidOperationException("OpenIddict application has no identifier.");

        var subject = request.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var scopes = request.Scopes.ToImmutableArray();

        // If the user already has a valid permanent authorization covering every requested scope,
        // leave it as-is. OpenIddict will match against it on the next /connect/authorize call.
        var existing = new List<object>();
        await foreach (var auth in authManager.FindAsync(
            subject: subject,
            client: appId,
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: scopes,
            cancellationToken: ct))
        {
            existing.Add(auth);
        }

        if (existing.Count == 0)
        {
            var identity = new ClaimsIdentity("OidcConsent");
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, subject));
            var principal = new ClaimsPrincipal(identity);

            await authManager.CreateAsync(
                principal: principal,
                subject: subject,
                client: appId,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: scopes,
                cancellationToken: ct);
        }

        await audit.RecordAsync(
            OidcAuditEventType.ConsentGranted,
            actorUserId: request.UserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new { scopes = request.Scopes, autoGranted = false },
            ct: ct);

        return Unit.Value;
    }
}
