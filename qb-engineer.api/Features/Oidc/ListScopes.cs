using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

public record ListScopesQuery(bool IncludeInactive) : IRequest<IReadOnlyList<ScopeListItem>>;

public record ScopeListItem(
    int Id,
    string Name,
    string DisplayName,
    string Description,
    string ClaimMappingsJson,
    string? ResourcesCsv,
    bool IsSystem,
    bool IsActive);

public class ListScopesHandler(AppDbContext db)
    : IRequestHandler<ListScopesQuery, IReadOnlyList<ScopeListItem>>
{
    public async Task<IReadOnlyList<ScopeListItem>> Handle(ListScopesQuery request, CancellationToken ct)
    {
        var query = db.OidcCustomScopes.AsNoTracking();
        if (!request.IncludeInactive)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query
            .OrderBy(s => s.IsSystem ? 0 : 1)
            .ThenBy(s => s.Name)
            .Select(s => new ScopeListItem(
                s.Id, s.Name, s.DisplayName, s.Description,
                s.ClaimMappingsJson, s.ResourcesCsv, s.IsSystem, s.IsActive))
            .ToListAsync(ct);
    }
}
