using FluentValidation;
using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Redeems a registration ticket (RFC 7591 Dynamic Client Registration) and creates a new OIDC
/// client in the <see cref="OidcClientStatus.Pending"/> state. Admin approval is required before
/// the client can complete an authorization flow. The raw client_secret and registration access
/// token are returned exactly once in the response; only hashes persist.
/// </summary>
public record RedeemTicketCommand(
    string RawTicket,
    string ClientName,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string>? PostLogoutRedirectUris,
    IReadOnlyList<string> Scopes,
    IReadOnlyList<string>? GrantTypes,
    IReadOnlyList<string>? ResponseTypes,
    string? TokenEndpointAuthMethod,
    string? SoftwareStatement,
    IReadOnlyList<string>? Contacts,
    string? ClientUri,
    string? LogoUri,
    string? TosUri,
    string? PolicyUri,
    string? CallerIp) : IRequest<RedeemTicketResponse>;

public record RedeemTicketResponse(
    string ClientId,
    string ClientSecret,
    string RegistrationAccessToken,
    DateTimeOffset IssuedAt,
    string ClientName,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string> PostLogoutRedirectUris,
    IReadOnlyList<string> Scopes,
    IReadOnlyList<string> GrantTypes,
    IReadOnlyList<string> ResponseTypes,
    string TokenEndpointAuthMethod,
    int TicketId);

public class RedeemTicketValidator : AbstractValidator<RedeemTicketCommand>
{
    public RedeemTicketValidator()
    {
        RuleFor(x => x.RawTicket).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RedirectUris).NotEmpty().WithMessage("At least one redirect_uri is required.");
        RuleForEach(x => x.RedirectUris).Must(BeAbsoluteHttpUri)
            .WithMessage("Each redirect_uri must be an absolute http(s) URI without fragment.");
        RuleFor(x => x.Scopes).NotEmpty().WithMessage("At least one scope must be requested.");
    }

    private static bool BeAbsoluteHttpUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.Fragment);
}

public class RedeemTicketHandler(
    AppDbContext db,
    IClock clock,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager,
    ISoftwareStatementValidator softwareStatementValidator)
    : IRequestHandler<RedeemTicketCommand, RedeemTicketResponse>
{
    public async Task<RedeemTicketResponse> Handle(RedeemTicketCommand request, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var hash = OidcCrypto.HashSha256(request.RawTicket);

        var ticket = await db.OidcRegistrationTickets
            .FirstOrDefaultAsync(t => t.TicketHash == hash, ct);

        if (ticket is null || ticket.Status != OidcTicketStatus.Issued)
        {
            await audit.RecordAsync(
                OidcAuditEventType.TicketRedeemed,
                actorIp: request.CallerIp,
                details: new { reason = "ticket_not_found_or_not_issued" },
                ct: ct);
            throw new OidcRegistrationException(
                OidcRegistrationException.Errors.InvalidToken,
                "Registration ticket is invalid, already redeemed, or revoked.");
        }

        if (ticket.ExpiresAt <= now)
        {
            ticket.Status = OidcTicketStatus.Expired;
            await db.SaveChangesAsync(ct);
            await audit.RecordAsync(
                OidcAuditEventType.TicketExpired,
                actorIp: request.CallerIp,
                ticketId: ticket.Id,
                ct: ct);
            throw new OidcRegistrationException(
                OidcRegistrationException.Errors.InvalidToken,
                "Registration ticket has expired.");
        }

        ValidateRedirectUris(request.RedirectUris, ticket.AllowedRedirectUriPrefix, nameof(request.RedirectUris));
        if (request.PostLogoutRedirectUris is { Count: > 0 })
        {
            var postPrefix = ticket.AllowedPostLogoutRedirectUriPrefix ?? ticket.AllowedRedirectUriPrefix;
            ValidateRedirectUris(request.PostLogoutRedirectUris, postPrefix, nameof(request.PostLogoutRedirectUris));
        }

        var allowedScopes = ticket.AllowedScopesCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var scope in request.Scopes)
        {
            if (!allowedScopes.Contains(scope))
            {
                throw new OidcRegistrationException(
                    OidcRegistrationException.Errors.InvalidClientMetadata,
                    $"Scope '{scope}' is not permitted by this registration ticket.");
            }
        }

        SoftwareStatementClaims? statementClaims = null;
        if (ticket.RequireSignedSoftwareStatement)
        {
            if (string.IsNullOrWhiteSpace(request.SoftwareStatement))
            {
                throw new OidcRegistrationException(
                    OidcRegistrationException.Errors.InvalidSoftwareStatement,
                    "Registration ticket requires a signed software_statement but none was presented.");
            }

            var trusted = (ticket.TrustedPublisherKeyIdsCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            statementClaims = await softwareStatementValidator.ValidateAsync(
                request.SoftwareStatement, trusted, ct);
            if (statementClaims is null)
            {
                await audit.RecordAsync(
                    OidcAuditEventType.InvalidSoftwareStatement,
                    actorIp: request.CallerIp,
                    ticketId: ticket.Id,
                    ct: ct);
                throw new OidcRegistrationException(
                    OidcRegistrationException.Errors.UnapprovedSoftwareStatement,
                    "software_statement could not be verified against a trusted publisher key.");
            }
        }

        var clientId = Guid.NewGuid().ToString("N");
        var clientSecretRaw = OidcCrypto.GenerateClientSecret();
        var registrationTokenRaw = OidcCrypto.GenerateRegistrationAccessToken();
        var tokenAuthMethod = request.TokenEndpointAuthMethod ?? "client_secret_basic";
        var grantTypes = request.GrantTypes is { Count: > 0 }
            ? request.GrantTypes.ToList()
            : new List<string> { "authorization_code", "refresh_token" };
        var responseTypes = request.ResponseTypes is { Count: > 0 }
            ? request.ResponseTypes.ToList()
            : new List<string> { "code" };
        var postLogoutRedirectUris = (request.PostLogoutRedirectUris ?? Array.Empty<string>()).ToList();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecretRaw,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            DisplayName = request.ClientName,
        };
        foreach (var uri in request.RedirectUris) descriptor.RedirectUris.Add(new Uri(uri));
        foreach (var uri in postLogoutRedirectUris) descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
        descriptor.Permissions.UnionWith(new[]
        {
            OpenIddictConstants.Permissions.Endpoints.Authorization,
            OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.Endpoints.EndSession,
            OpenIddictConstants.Permissions.Endpoints.Revocation,
            OpenIddictConstants.Permissions.Endpoints.Introspection,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
            OpenIddictConstants.Permissions.ResponseTypes.Code,
        });
        foreach (var scope in request.Scopes)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
        }
        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        await appManager.CreateAsync(descriptor, ct);

        var metadata = new OidcClientMetadata
        {
            ClientId = clientId,
            Status = OidcClientStatus.Pending,
            Description = $"Redeemed from ticket #{ticket.Id}. Expected client name: {ticket.ExpectedClientName}.",
            OwnerEmail = request.Contacts?.FirstOrDefault(),
            RequireConsent = true,
            IsFirstParty = false,
            RequiredRolesCsv = ticket.RequiredRolesForUsersCsv,
            CreatedByUserId = ticket.IssuedByUserId,
            RegistrationTicketId = ticket.Id,
            RegistrationAccessTokenHash = OidcCrypto.HashSha256(registrationTokenRaw),
            RegistrationAccessTokenRotatedAt = now,
            Notes = request.ClientUri is not null ? $"client_uri: {request.ClientUri}" : null,
        };
        db.OidcClientMetadata.Add(metadata);

        ticket.Status = OidcTicketStatus.Redeemed;
        ticket.RedeemedAt = now;
        ticket.RedeemedFromIp = request.CallerIp;
        ticket.ResultingClientId = clientId;

        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.TicketRedeemed,
            actorIp: request.CallerIp,
            clientId: clientId,
            ticketId: ticket.Id,
            ct: ct);
        await audit.RecordAsync(
            OidcAuditEventType.ClientRegistered,
            actorIp: request.CallerIp,
            clientId: clientId,
            ticketId: ticket.Id,
            details: new
            {
                request.ClientName,
                RedirectUris = request.RedirectUris,
                Scopes = request.Scopes,
                softwareStatementVerified = statementClaims is not null,
                publisherKeyId = statementClaims?.PublisherKeyId,
            },
            ct: ct);

        return new RedeemTicketResponse(
            clientId,
            clientSecretRaw,
            registrationTokenRaw,
            now,
            request.ClientName,
            request.RedirectUris,
            postLogoutRedirectUris,
            request.Scopes,
            grantTypes,
            responseTypes,
            tokenAuthMethod,
            ticket.Id);
    }

    private static void ValidateRedirectUris(IEnumerable<string> uris, string prefix, string fieldName)
    {
        foreach (var uri in uris)
        {
            if (!uri.StartsWith(prefix, StringComparison.Ordinal))
            {
                throw new OidcRegistrationException(
                    OidcRegistrationException.Errors.InvalidRedirectUri,
                    $"{fieldName} entry '{uri}' does not start with the ticket's allowed prefix '{prefix}'.");
            }
        }
    }
}
