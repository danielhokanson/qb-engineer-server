using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>Admin query for all registration tickets (filterable by status).</summary>
public record ListTicketsQuery(OidcTicketStatus? Status) : IRequest<IReadOnlyList<TicketListItem>>;

public record TicketListItem(
    int Id,
    string TicketPrefix,
    string ExpectedClientName,
    OidcTicketStatus Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RedeemedAt,
    int IssuedByUserId,
    string AllowedRedirectUriPrefix,
    string AllowedScopesCsv,
    bool RequireSignedSoftwareStatement,
    string? ResultingClientId,
    string? Notes);

public class ListTicketsHandler(AppDbContext db)
    : IRequestHandler<ListTicketsQuery, IReadOnlyList<TicketListItem>>
{
    public async Task<IReadOnlyList<TicketListItem>> Handle(ListTicketsQuery request, CancellationToken ct)
    {
        var query = db.OidcRegistrationTickets.AsNoTracking();
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        return await query
            .OrderByDescending(t => t.IssuedAt)
            .Select(t => new TicketListItem(
                t.Id,
                t.TicketPrefix,
                t.ExpectedClientName,
                t.Status,
                t.IssuedAt,
                t.ExpiresAt,
                t.RedeemedAt,
                t.IssuedByUserId,
                t.AllowedRedirectUriPrefix,
                t.AllowedScopesCsv,
                t.RequireSignedSoftwareStatement,
                t.ResultingClientId,
                t.Notes))
            .ToListAsync(ct);
    }
}
