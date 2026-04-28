namespace QBEngineer.Core.Entities;

/// <summary>
/// Phase 4 Phase-A — Stores tunable config payload for a single capability.
/// 1:0..1 with <see cref="Capability"/>. The payload is stored as raw JSON
/// (jsonb in Postgres) — typed parsing happens per-feature when the capability
/// uses it (Phase A keeps it untyped per implementation-decisions doc).
/// </summary>
public class CapabilityConfig : BaseAuditableEntity
{
    /// <summary>FK to <see cref="Capability"/>. Also serves as the natural unique key (one config per capability).</summary>
    public int CapabilityId { get; set; }
    public Capability? Capability { get; set; }

    /// <summary>JSON payload — raw text, parsed by capability-specific consumers. Empty object "{}" by default.</summary>
    public string ConfigJson { get; set; } = "{}";

    /// <summary>Schema version — increment when a capability's config shape changes (per 4D §2.1 §8.4).</summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>Optimistic concurrency token.</summary>
    public uint RowVersion { get; set; }
}
