using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

/// <summary>
/// Sidecar 1:1 extension of OpenIddict's OpenIddictApplications table.
/// Joined via <see cref="ClientId"/> (the OpenIddict "ClientId" string column).
/// Stores qb-engineer-specific fields that OpenIddict's base model doesn't carry:
/// lifecycle status, role gating, trust level, consent preference, and admin metadata.
/// </summary>
public class OidcClientMetadata : BaseAuditableEntity
{
    /// <summary>
    /// Stable client identifier string matching <c>OpenIddictApplication.ClientId</c>.
    /// This is the join column — NOT a foreign key to the int PK, because OpenIddict
    /// uses string GUIDs internally.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    public OidcClientStatus Status { get; set; } = OidcClientStatus.Pending;

    /// <summary>
    /// Human-readable description shown on admin list and consent screen.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Contact email for this integration (who to notify about rotations, security events, etc).
    /// </summary>
    public string? OwnerEmail { get; set; }

    /// <summary>
    /// When true, qb-engineer renders a consent screen to end users on first-party authorization.
    /// When false, consent is implicit (used for first-party + trusted clients).
    /// This is separate from OpenIddict's ConsentType to allow admin override.
    /// </summary>
    public bool RequireConsent { get; set; } = true;

    /// <summary>
    /// First-party clients belong to the same operator as qb-engineer.
    /// Used to suppress the third-party scary-warning UI on the consent screen.
    /// </summary>
    public bool IsFirstParty { get; set; }

    /// <summary>
    /// Comma-separated list of role names a user MUST have for login to succeed.
    /// Empty = any authenticated user may sign in.
    /// Checked at authorization time; denial is audited.
    /// </summary>
    public string? RequiredRolesCsv { get; set; }

    /// <summary>
    /// Comma-separated list of custom scope names this client is allowed to request.
    /// Applied IN ADDITION to OpenIddict's Permissions (which cover standard scopes).
    /// </summary>
    public string? AllowedCustomScopesCsv { get; set; }

    public int? CreatedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? LastSecretRotatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// SHA-256 hash of the active registration access token (RFC 7592).
    /// Clients present this to manage themselves via <c>PUT/DELETE /connect/register/{client_id}</c>.
    /// Auto-rotates on each management call. Raw token returned exactly once.
    /// </summary>
    public string? RegistrationAccessTokenHash { get; set; }

    public DateTimeOffset? RegistrationAccessTokenRotatedAt { get; set; }

    /// <summary>
    /// Ticket this client was redeemed from (if any). Null for clients created by seed or direct admin action.
    /// </summary>
    public int? RegistrationTicketId { get; set; }
    public OidcRegistrationTicket? RegistrationTicket { get; set; }

    public string? Notes { get; set; }
}
