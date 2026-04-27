using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

/// <summary>
/// Query parameters for <c>GET /api/v1/parts</c>. Phase 3 F7-partial / WU-17
/// standardises pagination, sort, search, and the part-specific filters
/// around a single bound model.
///
/// Backward compat: callers passing the legacy <c>status</c> / <c>type</c> /
/// <c>search</c> query params continue to work — the controller plumbs both
/// old and new names through to the underlying handler.
/// </summary>
public record PartListQuery : PagedQuery
{
    /// <summary>Lifecycle status (Active / Draft / Prototype / Obsolete).</summary>
    public PartStatus? Status { get; init; }

    /// <summary>Activation alias — <c>true</c> excludes Obsolete; <c>false</c> shows Obsolete only.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Part type (Part / Assembly / RawMaterial / Consumable / Tooling / Fastener / Electronic / Packaging).</summary>
    public PartType? Type { get; init; }

    /// <summary>Restrict to parts whose preferred-vendor matches this id.</summary>
    public int? DefaultVendorId { get; init; }
}
