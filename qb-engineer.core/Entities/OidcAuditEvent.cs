using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Entities;

/// <summary>
/// Append-only audit record for every security-relevant OIDC provider event.
/// Shown to admins in the OIDC client detail page and exported for compliance.
/// Never soft-deleted — uses <see cref="BaseEntity"/> so the global soft-delete filter doesn't apply.
/// </summary>
public class OidcAuditEvent : BaseEntity
{
    public OidcAuditEventType EventType { get; set; }

    /// <summary>User who performed the action. Null when the event is system-triggered (ticket expiry, etc).</summary>
    public int? ActorUserId { get; set; }

    public string? ActorIpAddress { get; set; }

    /// <summary>OpenIddict ClientId of the client the event concerns. Null for scope/ticket-only events.</summary>
    public string? ClientId { get; set; }

    public int? TicketId { get; set; }

    public string? ScopeName { get; set; }

    /// <summary>Freeform JSON payload with event-specific data (redirect URI attempted, secret prefix, etc).</summary>
    public string? DetailsJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
