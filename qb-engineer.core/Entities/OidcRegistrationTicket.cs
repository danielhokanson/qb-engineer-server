using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

/// <summary>
/// Single-use, time-limited, admin-minted credential that a client application
/// redeems at <c>POST /connect/register</c> to obtain credentials and enter the
/// Pending registration state.
///
/// TOFU mitigation: the raw ticket is shown to the admin exactly once on mint,
/// then only a SHA-256 hash is persisted. At redemption the caller presents the
/// raw value and it's hashed + compared. Redirect URIs, scopes, and role gates
/// are pre-constrained here so a leaked ticket cannot register arbitrary clients.
/// </summary>
public class OidcRegistrationTicket : BaseAuditableEntity
{
    /// <summary>First 8 chars of the raw ticket, stored for UI display only.</summary>
    public string TicketPrefix { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the raw ticket. The raw value is never persisted.</summary>
    public string TicketHash { get; set; } = string.Empty;

    public int IssuedByUserId { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RedeemedAt { get; set; }
    public string? RedeemedFromIp { get; set; }

    /// <summary>Required prefix for every redirect URI the client declares at redemption.</summary>
    public string AllowedRedirectUriPrefix { get; set; } = string.Empty;

    public string? AllowedPostLogoutRedirectUriPrefix { get; set; }

    /// <summary>Comma-separated scope allowlist. Client cannot request scopes outside this set.</summary>
    public string AllowedScopesCsv { get; set; } = string.Empty;

    /// <summary>Comma-separated role names required of any user who signs in through the resulting client.</summary>
    public string? RequiredRolesForUsersCsv { get; set; }

    /// <summary>
    /// If true, the client must present a signed software_statement JWT at /connect/register.
    /// The JWT issuer must be listed in <see cref="TrustedPublisherKeyIdsCsv"/>.
    /// </summary>
    public bool RequireSignedSoftwareStatement { get; set; }

    /// <summary>Comma-separated list of JWKS key IDs trusted to sign software statements for this ticket.</summary>
    public string? TrustedPublisherKeyIdsCsv { get; set; }

    /// <summary>Admin-declared client name hint (for diff review when the client submits its claimed name).</summary>
    public string ExpectedClientName { get; set; } = string.Empty;

    /// <summary>Admin-visible memo; shown in the audit feed.</summary>
    public string? Notes { get; set; }

    public OidcTicketStatus Status { get; set; } = OidcTicketStatus.Issued;

    /// <summary>OpenIddict ClientId of the client created from this ticket (after redemption).</summary>
    public string? ResultingClientId { get; set; }
}
