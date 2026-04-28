using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Api.Capabilities;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Capabilities.Descriptor;

/// <summary>
/// Phase 4 Phase-A — Read-only capability descriptor query. Returns the full
/// capability set with current state, defaults, and role hints. Authenticated
/// users only (per 4D decision #2 and 4D §5.1). The UI consumes this on
/// post-login bootstrap and on capability-changed SignalR events (Phase C).
/// </summary>
public record GetCapabilityDescriptorQuery() : IRequest<CapabilityDescriptorResponseModel>;

public record CapabilityDescriptorResponseModel(
    DateTimeOffset GeneratedAt,
    int TotalCount,
    int EnabledCount,
    IReadOnlyList<CapabilityDescriptorEntry> Capabilities);

public record CapabilityDescriptorEntry(
    string Id,
    string Code,
    string Area,
    string Name,
    string Description,
    bool Enabled,
    bool IsDefaultOn,
    string? RequiresRoles);

public class GetCapabilityDescriptorHandler(AppDbContext db, ICapabilitySnapshotProvider snapshots)
    : IRequestHandler<GetCapabilityDescriptorQuery, CapabilityDescriptorResponseModel>
{
    public async Task<CapabilityDescriptorResponseModel> Handle(
        GetCapabilityDescriptorQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await db.Capabilities
            .AsNoTracking()
            .OrderBy(c => c.Area).ThenBy(c => c.Code)
            .Select(c => new CapabilityDescriptorEntry(
                c.Code,        // surface "id" + "code" interchangeably; clients can use either
                c.Code,
                c.Area,
                c.Name,
                c.Description,
                c.Enabled,
                c.IsDefaultOn,
                c.RequiresRoles))
            .ToListAsync(cancellationToken);

        var enabledCount = rows.Count(r => r.Enabled);

        return new CapabilityDescriptorResponseModel(
            GeneratedAt: snapshots.Current.GeneratedAt,
            TotalCount: rows.Count,
            EnabledCount: enabledCount,
            Capabilities: rows);
    }
}
