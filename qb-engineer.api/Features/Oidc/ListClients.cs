using MediatR;

using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>Admin list of all registered clients (Pending, Active, Suspended, Revoked).</summary>
public record ListClientsQuery(OidcClientStatus? Status) : IRequest<IReadOnlyList<ClientListItem>>;

public record ClientListItem(
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
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? LastSecretRotatedAt,
    int? RegistrationTicketId);

public class ListClientsHandler(AppDbContext db, IOpenIddictApplicationManager appManager)
    : IRequestHandler<ListClientsQuery, IReadOnlyList<ClientListItem>>
{
    public async Task<IReadOnlyList<ClientListItem>> Handle(ListClientsQuery request, CancellationToken ct)
    {
        var query = db.OidcClientMetadata.AsNoTracking();
        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        var metadata = await query.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
        var results = new List<ClientListItem>(metadata.Count);

        foreach (var m in metadata)
        {
            var app = await appManager.FindByClientIdAsync(m.ClientId, ct);
            var displayName = app is null ? null : await appManager.GetDisplayNameAsync(app, ct);
            results.Add(new ClientListItem(
                m.ClientId,
                displayName,
                m.Status,
                m.Description,
                m.OwnerEmail,
                m.RequireConsent,
                m.IsFirstParty,
                m.RequiredRolesCsv,
                m.AllowedCustomScopesCsv,
                m.CreatedAt,
                m.ApprovedAt,
                m.LastUsedAt,
                m.LastSecretRotatedAt,
                m.RegistrationTicketId));
        }

        return results;
    }
}
