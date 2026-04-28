using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Capabilities;

/// <summary>
/// Phase 4 Phase-A — Idempotent seeder for the capabilities table.
///
/// Stable-ID upsert pattern (4F decision #4):
///   • New IDs (not yet in DB) are INSERTed with Enabled = IsDefaultOn.
///   • Existing rows: only metadata (Name, Description, Area, IsDefaultOn,
///     RequiresRoles) is refreshed. Enabled is NEVER overwritten — admin
///     state is operator-owned.
///
/// Runs at startup after EF migrations and before the snapshot is hydrated.
/// </summary>
public interface ICapabilityCatalogSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}

public class CapabilityCatalogSeeder(AppDbContext db, ILogger<CapabilityCatalogSeeder> logger) : ICapabilityCatalogSeeder
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Suppress audit during seed — these are system-owned rows.
        var prevSuppress = db.SuppressAudit;
        db.SuppressAudit = true;
        try
        {
            var existing = await db.Capabilities.ToDictionaryAsync(c => c.Code, ct);
            var inserted = 0;
            var refreshed = 0;

            foreach (var def in CapabilityCatalog.All)
            {
                if (existing.TryGetValue(def.Code, out var row))
                {
                    // Refresh metadata only; never touch Enabled.
                    var changed = false;
                    if (row.Area != def.Area) { row.Area = def.Area; changed = true; }
                    if (row.Name != def.Name) { row.Name = def.Name; changed = true; }
                    if (row.Description != def.Description) { row.Description = def.Description; changed = true; }
                    if (row.IsDefaultOn != def.IsDefaultOn) { row.IsDefaultOn = def.IsDefaultOn; changed = true; }
                    if (row.RequiresRoles != def.RequiresRoles) { row.RequiresRoles = def.RequiresRoles; changed = true; }
                    if (changed) refreshed++;
                }
                else
                {
                    db.Capabilities.Add(new Capability
                    {
                        Code = def.Code,
                        Area = def.Area,
                        Name = def.Name,
                        Description = def.Description,
                        Enabled = def.IsDefaultOn,
                        IsDefaultOn = def.IsDefaultOn,
                        RequiresRoles = def.RequiresRoles,
                    });
                    inserted++;
                }
            }

            if (inserted > 0 || refreshed > 0)
            {
                await db.SaveChangesAsync(ct);
            }

            logger.LogInformation(
                "[CAPABILITY-SEED] Catalog seed complete: inserted={Inserted}, refreshed={Refreshed}, total={Total}",
                inserted, refreshed, CapabilityCatalog.All.Count);
        }
        finally
        {
            db.SuppressAudit = prevSuppress;
        }
    }
}
