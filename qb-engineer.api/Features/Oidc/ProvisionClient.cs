using FluentValidation;
using MediatR;

using OpenIddict.Abstractions;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Admin-driven direct client provisioning — bypasses the RFC 7591 ticket-redemption round-trip
/// for apps the admin owns themselves. Creates an OpenIddict application and returns the
/// client_id/client_secret/registration_access_token inline so the admin can paste them into
/// the external app's configuration. The admin chooses the initial status: Pending keeps the
/// existing two-step ceremony; Active approves in one shot.
/// </summary>
public record ProvisionClientCommand(
    string ClientName,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string>? PostLogoutRedirectUris,
    IReadOnlyList<string> Scopes,
    bool ApproveImmediately,
    bool IsFirstParty,
    bool RequireConsent,
    string? RequiredRolesCsv,
    string? OwnerEmail,
    string? Description,
    string? Notes,
    int ActorUserId,
    string? ActorIp) : IRequest<ProvisionClientResponse>;

public record ProvisionClientResponse(
    string ClientId,
    string ClientSecret,
    string RegistrationAccessToken,
    DateTimeOffset IssuedAt,
    string ClientName,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string> PostLogoutRedirectUris,
    IReadOnlyList<string> Scopes,
    string Status);

public class ProvisionClientValidator : AbstractValidator<ProvisionClientCommand>
{
    public ProvisionClientValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RedirectUris).NotEmpty().WithMessage("At least one redirect URI is required.");
        RuleForEach(x => x.RedirectUris).Must(BeAbsoluteHttpUri)
            .WithMessage("Each redirect_uri must be an absolute http(s) URI without fragment.");
        RuleForEach(x => x.PostLogoutRedirectUris!)
            .Must(BeAbsoluteHttpUri)
            .When(x => x.PostLogoutRedirectUris is { Count: > 0 })
            .WithMessage("Each post-logout URI must be an absolute http(s) URI without fragment.");
        RuleFor(x => x.Scopes).NotEmpty().WithMessage("At least one scope must be granted.");
        RuleFor(x => x.OwnerEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.OwnerEmail))
            .WithMessage("Owner email is not a valid email address.");
    }

    private static bool BeAbsoluteHttpUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.Fragment);
}

public class ProvisionClientHandler(
    AppDbContext db,
    IClock clock,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager)
    : IRequestHandler<ProvisionClientCommand, ProvisionClientResponse>
{
    public async Task<ProvisionClientResponse> Handle(ProvisionClientCommand request, CancellationToken ct)
    {
        var now = clock.UtcNow;

        var clientId = Guid.NewGuid().ToString("N");
        var clientSecretRaw = OidcCrypto.GenerateClientSecret();
        var registrationTokenRaw = OidcCrypto.GenerateRegistrationAccessToken();
        var postLogoutRedirectUris = (request.PostLogoutRedirectUris ?? Array.Empty<string>()).ToList();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientSecretRaw,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = request.RequireConsent
                ? OpenIddictConstants.ConsentTypes.Explicit
                : OpenIddictConstants.ConsentTypes.Implicit,
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

        var status = request.ApproveImmediately ? OidcClientStatus.Active : OidcClientStatus.Pending;
        var metadata = new OidcClientMetadata
        {
            ClientId = clientId,
            Status = status,
            Description = request.Description,
            OwnerEmail = request.OwnerEmail,
            RequireConsent = request.RequireConsent,
            IsFirstParty = request.IsFirstParty,
            RequiredRolesCsv = string.IsNullOrWhiteSpace(request.RequiredRolesCsv) ? null : request.RequiredRolesCsv,
            CreatedByUserId = request.ActorUserId,
            RegistrationTicketId = null, // direct-provision — no ticket
            RegistrationAccessTokenHash = OidcCrypto.HashSha256(registrationTokenRaw),
            RegistrationAccessTokenRotatedAt = now,
            ApprovedAt = request.ApproveImmediately ? now : null,
            ApprovedByUserId = request.ApproveImmediately ? request.ActorUserId : null,
            Notes = request.Notes,
        };
        db.OidcClientMetadata.Add(metadata);
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ClientRegistered,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: clientId,
            details: new
            {
                request.ClientName,
                RedirectUris = request.RedirectUris,
                Scopes = request.Scopes,
                provisionMode = "direct-admin",
                initialStatus = status.ToString(),
            },
            ct: ct);

        if (request.ApproveImmediately)
        {
            await audit.RecordAsync(
                OidcAuditEventType.ClientApproved,
                actorUserId: request.ActorUserId,
                actorIp: request.ActorIp,
                clientId: clientId,
                details: new { request.IsFirstParty, request.RequireConsent, request.RequiredRolesCsv },
                ct: ct);
        }

        return new ProvisionClientResponse(
            clientId,
            clientSecretRaw,
            registrationTokenRaw,
            now,
            request.ClientName,
            request.RedirectUris,
            postLogoutRedirectUris,
            request.Scopes,
            status.ToString());
    }
}
