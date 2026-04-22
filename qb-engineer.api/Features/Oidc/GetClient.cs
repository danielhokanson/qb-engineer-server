using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public record GetClientQuery(string ClientId) : IRequest<ClientDetailResponse>;

public record ClientDetailResponse(
    string ClientId,
    string? DisplayName,
    OidcClientStatus Status,
    string? Description,
    string? OwnerEmail,
    bool RequireConsent,
    bool IsFirstParty,
    string? RequiredRolesCsv,
    string? AllowedCustomScopesCsv,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ApprovedAt,
    int? ApprovedByUserId,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? LastSecretRotatedAt,
    int? RegistrationTicketId,
    string? Notes,
    IReadOnlyList<string> RedirectUris,
    IReadOnlyList<string> PostLogoutRedirectUris,
    IReadOnlyList<string> Permissions);

public class GetClientHandler(AppDbContext db, IOpenIddictApplicationManager appManager)
    : IRequestHandler<GetClientQuery, ClientDetailResponse>
{
    public async Task<ClientDetailResponse> Handle(GetClientQuery request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        var app = await appManager.FindByClientIdAsync(request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OpenIddict application '{request.ClientId}' not found.");

        var displayName = await appManager.GetDisplayNameAsync(app, ct);
        var redirectUris = await appManager.GetRedirectUrisAsync(app, ct);
        var postLogoutUris = await appManager.GetPostLogoutRedirectUrisAsync(app, ct);
        var permissions = await appManager.GetPermissionsAsync(app, ct);

        return new ClientDetailResponse(
            metadata.ClientId,
            displayName,
            metadata.Status,
            metadata.Description,
            metadata.OwnerEmail,
            metadata.RequireConsent,
            metadata.IsFirstParty,
            metadata.RequiredRolesCsv,
            metadata.AllowedCustomScopesCsv,
            metadata.CreatedAt,
            metadata.ApprovedAt,
            metadata.ApprovedByUserId,
            metadata.LastUsedAt,
            metadata.LastSecretRotatedAt,
            metadata.RegistrationTicketId,
            metadata.Notes,
            redirectUris.ToArray(),
            postLogoutUris.ToArray(),
            permissions.ToArray());
    }
}
