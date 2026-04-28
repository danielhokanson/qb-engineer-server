namespace QBEngineer.Core.Entities;

/// <summary>
/// Phase 4 Phase-A — Storage row for a single capability from the 4A catalog.
/// One row per stable capability code (e.g. "CAP-MD-CUSTOMERS"). The seeder
/// upserts on <see cref="Code"/> and never overwrites <see cref="Enabled"/>
/// once the row exists (admin-changed state is operator-owned).
/// </summary>
public class Capability : BaseAuditableEntity
{
    /// <summary>Stable capability identifier from the 4A catalog (e.g. "CAP-MD-CUSTOMERS"). Immutable once seeded.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Functional area code (IDEN, MD, P2P, O2C, MFG, PLAN, INV, QC, MAINT, ACCT, HR, RPT, CROSS, EXT).</summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>Human display name from the catalog.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Catalog description. Stored in DB so admin UI can render even when catalog file is unavailable.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Current operator state — true = enabled, false = disabled. Mutated by admin endpoints (Phase C).</summary>
    public bool Enabled { get; set; }

    /// <summary>The catalog default-on flag, preserved so "reset to defaults" remains meaningful.</summary>
    public bool IsDefaultOn { get; set; }

    /// <summary>
    /// Optional comma-separated role list that may toggle / configure this capability.
    /// Null = no role restriction beyond the standard admin endpoint guard.
    /// Used by the descriptor surface so the UI can hide capabilities the user can't manage.
    /// </summary>
    public string? RequiresRoles { get; set; }

    /// <summary>Optimistic concurrency token (Postgres xmin / EF Core RowVersion).</summary>
    public uint RowVersion { get; set; }

    public ICollection<CapabilityConfig> Configs { get; set; } = [];
}
