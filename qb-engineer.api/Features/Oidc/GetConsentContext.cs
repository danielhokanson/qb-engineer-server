using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Returns the information the consent screen needs: client display info + a description for every
/// requested scope (system or custom). Used by the Angular <c>OidcConsentComponent</c> before rendering.
/// </summary>
public record GetConsentContextQuery(
    string ClientId,
    IReadOnlyCollection<string> RequestedScopes) : IRequest<ConsentContextResponse>;

public record ConsentContextResponse(
    string ClientId,
    string? ClientDisplayName,
    string? ClientDescription,
    string? OwnerEmail,
    bool IsFirstParty,
    IReadOnlyList<ConsentScopeDescriptor> Scopes);

public record ConsentScopeDescriptor(
    string Name,
    string DisplayName,
    string Description,
    bool IsSystem);

public class GetConsentContextHandler(
    AppDbContext db,
    IOpenIddictApplicationManager appManager)
    : IRequestHandler<GetConsentContextQuery, ConsentContextResponse>
{
    public async Task<ConsentContextResponse> Handle(GetConsentContextQuery request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status != OidcClientStatus.Active)
        {
            throw new InvalidOperationException("This client is not currently allowed to sign users in.");
        }

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct);
        var displayName = app is null ? null : await appManager.GetDisplayNameAsync(app, ct);

        var customScopes = await db.OidcCustomScopes.AsNoTracking()
            .Where(s => request.RequestedScopes.Contains(s.Name) && s.IsActive)
            .ToListAsync(ct);

        var scopes = request.RequestedScopes
            .Select(name =>
            {
                var custom = customScopes.FirstOrDefault(s => s.Name == name);
                if (custom is not null)
                {
                    return new ConsentScopeDescriptor(
                        custom.Name,
                        string.IsNullOrWhiteSpace(custom.DisplayName) ? custom.Name : custom.DisplayName,
                        custom.Description ?? string.Empty,
                        custom.IsSystem);
                }
                return new ConsentScopeDescriptor(name, name, DefaultSystemDescription(name), IsSystem: true);
            })
            .ToList();

        return new ConsentContextResponse(
            metadata.ClientId,
            displayName,
            metadata.Description,
            metadata.OwnerEmail,
            metadata.IsFirstParty,
            scopes);
    }

    private static string DefaultSystemDescription(string scope) => scope switch
    {
        "openid" => "Sign you in and issue an identifier.",
        "profile" => "See your display name and username.",
        "email" => "See your email address.",
        "offline_access" => "Stay signed in and refresh access without asking you again.",
        "roles" => "See your role memberships.",
        _ => "Access information associated with this scope.",
    };
}

