namespace QBEngineer.Core.Entities;

public class SalesTaxRate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// 2-letter US state code (e.g. "CA", "TX"). Null = general/default rate.
    /// Used for automatic lookup based on customer's ship-to state.
    /// </summary>
    public string? StateCode { get; set; }
    /// <summary>
    /// The effective combined rate (state + typical local), stored as a decimal fraction.
    /// E.g. 0.0725 = 7.25%. Admins should set this to the actual combined rate
    /// for their nexus jurisdictions. Local rates vary by city/county — see state tax authority.
    /// </summary>
    public decimal Rate { get; set; }
    /// <summary>When this rate takes effect (UTC). Used to schedule future rate changes.</summary>
    public DateTime EffectiveFrom { get; set; }
    /// <summary>When this rate expires (UTC). Null = currently active.</summary>
    public DateTime? EffectiveTo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
