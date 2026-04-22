namespace QBEngineer.Core.Entities;

/// <summary>
/// qb-engineer-defined OIDC scope that maps to one or more claims in the ID token / userinfo response.
/// Separate from OpenIddict's <c>OpenIddictScopes</c> table — we use that for protocol-level scope
/// discovery and <see cref="OidcCustomScope"/> for our claim-mapping logic.
///
/// Example: scope "qb:read-jobs" → emits claim "qb_permissions" = "jobs:read" when user has role Engineer.
/// </summary>
public class OidcCustomScope : BaseAuditableEntity
{
    /// <summary>Scope name as requested in the authorize request's <c>scope</c> parameter (e.g. <c>qb:read-jobs</c>).</summary>
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Shown on the consent screen — tell the user what this scope grants.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of claim mapping rules: each rule emits a claim based on user state.
    /// Schema:
    /// <code>
    /// [{ "claimType": "qb_permissions",
    ///    "source": "role",             // "role" | "profile" | "static" | "expression"
    ///    "value": "Engineer",          // for "role": required role; for "static": literal value
    ///    "emittedValue": "jobs:read"   // optional override; defaults to source value
    /// }]
    /// </code>
    /// </summary>
    public string ClaimMappingsJson { get; set; } = "[]";

    /// <summary>Comma-separated resource identifiers (APIs) that should accept tokens with this scope.</summary>
    public string? ResourcesCsv { get; set; }

    /// <summary>System scopes are openid/profile/email/offline_access — cannot be edited or deleted by admins.</summary>
    public bool IsSystem { get; set; }

    public bool IsActive { get; set; } = true;
}
