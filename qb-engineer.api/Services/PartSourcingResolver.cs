using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Services;

/// <summary>
/// Pillar 3 — default <see cref="IPartSourcingResolver"/> implementation.
/// Reads from <c>VendorPart</c> with <c>IsPreferred=true</c> and falls back
/// to the <c>Part</c> snapshot columns. Soft-deleted VendorPart rows are
/// skipped via the global query filter on BaseEntity. All queries use
/// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking{T}"/>.
/// </summary>
public class PartSourcingResolver(AppDbContext db) : IPartSourcingResolver
{
    public async Task<PartSourcingValues> ResolveAsync(int partId, CancellationToken ct)
    {
        var preferred = await db.VendorParts
            .AsNoTracking()
            .Where(vp => vp.PartId == partId && vp.IsPreferred)
            .Select(vp => new
            {
                vp.VendorId,
                vp.LeadTimeDays,
                vp.MinOrderQty,
                vp.PackSize,
            })
            .FirstOrDefaultAsync(ct);

        var snapshot = await db.Parts
            .AsNoTracking()
            .Where(p => p.Id == partId)
            .Select(p => new
            {
                p.LeadTimeDays,
                p.MinOrderQty,
                p.PackSize,
            })
            .FirstOrDefaultAsync(ct);

        // Coalesce per-column: prefer VendorPart override, then Part snapshot.
        var leadTime = preferred?.LeadTimeDays ?? snapshot?.LeadTimeDays;
        var minOrderQty = preferred?.MinOrderQty
            ?? (snapshot?.MinOrderQty.HasValue == true ? (decimal?)snapshot.MinOrderQty.Value : null);
        var packSize = preferred?.PackSize
            ?? (snapshot?.PackSize.HasValue == true ? (decimal?)snapshot.PackSize.Value : null);

        return new PartSourcingValues(
            PartId: partId,
            PreferredVendorId: preferred?.VendorId,
            LeadTimeDays: leadTime,
            MinOrderQty: minOrderQty,
            PackSize: packSize,
            ResolvedFromVendorPart: preferred is not null);
    }

    public async Task<IReadOnlyDictionary<int, PartSourcingValues>> ResolveManyAsync(
        IReadOnlyCollection<int> partIds, CancellationToken ct)
    {
        if (partIds.Count == 0)
            return new Dictionary<int, PartSourcingValues>();

        // De-dup defensively — callers may not always pass a HashSet.
        var idList = partIds.Distinct().ToList();

        // One query for all preferred VendorPart rows.
        var preferredRows = await db.VendorParts
            .AsNoTracking()
            .Where(vp => idList.Contains(vp.PartId) && vp.IsPreferred)
            .Select(vp => new
            {
                vp.PartId,
                vp.VendorId,
                vp.LeadTimeDays,
                vp.MinOrderQty,
                vp.PackSize,
            })
            .ToListAsync(ct);

        var preferredByPart = preferredRows.ToDictionary(r => r.PartId);

        // One query for the Part snapshot columns.
        var snapshotRows = await db.Parts
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.LeadTimeDays,
                p.MinOrderQty,
                p.PackSize,
            })
            .ToListAsync(ct);

        var snapshotByPart = snapshotRows.ToDictionary(r => r.Id);

        var result = new Dictionary<int, PartSourcingValues>(idList.Count);

        foreach (var id in idList)
        {
            preferredByPart.TryGetValue(id, out var preferred);
            snapshotByPart.TryGetValue(id, out var snapshot);

            var leadTime = preferred?.LeadTimeDays ?? snapshot?.LeadTimeDays;
            var minOrderQty = preferred?.MinOrderQty
                ?? (snapshot?.MinOrderQty.HasValue == true ? (decimal?)snapshot.MinOrderQty.Value : null);
            var packSize = preferred?.PackSize
                ?? (snapshot?.PackSize.HasValue == true ? (decimal?)snapshot.PackSize.Value : null);

            result[id] = new PartSourcingValues(
                PartId: id,
                PreferredVendorId: preferred?.VendorId,
                LeadTimeDays: leadTime,
                MinOrderQty: minOrderQty,
                PackSize: packSize,
                ResolvedFromVendorPart: preferred is not null);
        }

        return result;
    }
}
