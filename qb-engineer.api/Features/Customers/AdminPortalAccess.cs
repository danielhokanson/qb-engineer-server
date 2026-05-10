using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;
using QBEngineer.Data.Extensions;

namespace QBEngineer.Api.Features.Customers;

/// <summary>
/// Phase 1r — admin oversight of customer-portal access. Lists every
/// CustomerPortalAccess row across all customers with last-login
/// timestamp + IsEnabled flag. Powers /customers/portal-access.
/// </summary>
public record ListPortalAccessQuery() : IRequest<List<PortalAccessRowModel>>;

public class ListPortalAccessHandler(AppDbContext db) : IRequestHandler<ListPortalAccessQuery, List<PortalAccessRowModel>>
{
    public async Task<List<PortalAccessRowModel>> Handle(ListPortalAccessQuery request, CancellationToken ct)
    {
        return await (
            from a in db.CustomerPortalAccesses.AsNoTracking()
            join c in db.Contacts.AsNoTracking() on a.ContactId equals c.Id
            join cust in db.Customers.AsNoTracking() on a.CustomerId equals cust.Id
            orderby a.LastLoginAt descending
            select new PortalAccessRowModel(
                a.Id, a.ContactId, a.CustomerId, cust.Name,
                c.FirstName, c.LastName, c.Email,
                a.IsEnabled, a.LastLoginAt, a.CreatedAt)
        ).ToListAsync(ct);
    }
}

public record SetPortalAccessEnabledCommand(int AccessId, bool Enabled) : IRequest;

public class SetPortalAccessEnabledHandler(AppDbContext db) : IRequestHandler<SetPortalAccessEnabledCommand>
{
    public async Task Handle(SetPortalAccessEnabledCommand request, CancellationToken ct)
    {
        var access = await db.CustomerPortalAccesses.FirstOrDefaultAsync(a => a.Id == request.AccessId, ct)
            ?? throw new KeyNotFoundException($"Portal access {request.AccessId} not found.");
        if (access.IsEnabled == request.Enabled) return;
        access.IsEnabled = request.Enabled;
        // If revoking, clear any pending magic-link token so the contact can't bypass.
        if (!request.Enabled)
        {
            access.OneTimeTokenHash = null;
            access.OneTimeTokenExpiresAt = null;
        }
        db.LogActivityAt(
            request.Enabled ? "portal-access-enabled" : "portal-access-revoked",
            request.Enabled ? "Portal access enabled by admin" : "Portal access revoked by admin",
            ("Contact", access.ContactId), ("Customer", access.CustomerId));
        await db.SaveChangesAsync(ct);
    }
}
