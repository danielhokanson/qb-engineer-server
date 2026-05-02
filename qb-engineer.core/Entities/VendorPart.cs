namespace QBEngineer.Core.Entities;

/// <summary>
/// Pillar 3 — Intersection entity capturing the (Vendor, Part) relationship
/// with vendor-scoped sourcing metadata. Replaces the pattern of stamping a
/// "preferred vendor's values" snapshot directly onto <c>Part</c>.
///
/// One Part may have many VendorPart rows (multi-source AVL); one VendorPart
/// references one Vendor and one Part. Tiered pricing lives on the
/// <see cref="VendorPartPriceTier"/> child collection.
///
/// Several columns that used to live on <c>Part</c> moved here:
/// <c>MinOrderQty</c>, <c>PackSize</c>, <c>LeadTimeDays</c>. The Part
/// columns remain (for now) as a snapshot of the preferred vendor's values
/// — Phase B's migration backfills VendorPart rows from those snapshots.
/// </summary>
public class VendorPart : BaseAuditableEntity
{
    public int VendorId { get; set; }
    public int PartId { get; set; }

    /// <summary>
    /// The vendor's SKU for this part (the catalog/order number you'd put on
    /// a PO line — what users call the "external part number").
    /// </summary>
    public string? VendorPartNumber { get; set; }

    /// <summary>
    /// OEM company name as reported by this vendor (the brand name on the
    /// part). Lives on VendorPart rather than Part because the OEM may
    /// differ between distributors of the same logical part (counterfeit
    /// vs. authentic, alternate manufacturers, etc.).
    /// </summary>
    public string? ManufacturerName { get; set; }

    /// <summary>
    /// Manufacturer Part Number — the OEM's identifier for the part as
    /// reported by this vendor. Customer drawings + datasheets reference
    /// this value, not the in-house SKU. Lives on VendorPart so each
    /// distributor can carry its own manufacturer identity (legacy column
    /// name on the row is <c>vendor_mpn</c>; the conceptual rename to
    /// "manufacturer part number" landed when OEM identity moved off Part).
    /// </summary>
    public string? VendorMpn { get; set; }

    /// <summary>Per-vendor lead time in days from PO to dock.</summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>Per-vendor minimum order quantity (in vendor's purchase UoM).</summary>
    public decimal? MinOrderQty { get; set; }

    /// <summary>Per-vendor pack size (e.g., box of 100, case of 12).</summary>
    public decimal? PackSize { get; set; }

    /// <summary>
    /// Per-vendor nominal country of origin (vendor's claim). Per-receipt
    /// actual COO lives on <c>LotRecord.CountryOfOrigin</c> when lot-tracked.
    /// ISO-3166 alpha-2 code (e.g., 'US', 'CN', 'DE').
    /// </summary>
    public string? CountryOfOrigin { get; set; }

    /// <summary>
    /// Vendor-specific HTS code (rare — usually the part's HTS doesn't change
    /// by vendor, but some specialty cases warrant it).
    /// </summary>
    public string? HtsCode { get; set; }

    /// <summary>
    /// Approved-vendor-list (AVL) flag — engineering or quality has signed
    /// off on this vendor as a valid source. Defaults true on direct user
    /// creation; admin tooling can revoke.
    /// </summary>
    public bool IsApproved { get; set; } = true;

    /// <summary>
    /// Preferred-vendor flag — at most one VendorPart per Part should be
    /// preferred. Drives auto-PO defaults and cost-of-goods rollups.
    /// </summary>
    public bool IsPreferred { get; set; }

    /// <summary>Any vendor-specific certifications attested for this part (free-text JSON).</summary>
    public string? Certifications { get; set; }

    /// <summary>Most recent quote date — drives "quote may be stale" UI hints.</summary>
    public DateTimeOffset? LastQuotedDate { get; set; }

    public string? Notes { get; set; }

    public Vendor Vendor { get; set; } = null!;
    public Part Part { get; set; } = null!;

    public ICollection<VendorPartPriceTier> PriceTiers { get; set; } = [];
}
