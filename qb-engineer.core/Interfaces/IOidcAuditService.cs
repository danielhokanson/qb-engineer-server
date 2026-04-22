using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Interfaces;

/// <summary>
/// Append-only audit sink for OIDC provider events. Each call inserts one row into <c>oidc_audit_events</c>.
/// </summary>
public interface IOidcAuditService
{
    Task RecordAsync(
        OidcAuditEventType eventType,
        int? actorUserId = null,
        string? actorIp = null,
        string? clientId = null,
        int? ticketId = null,
        string? scopeName = null,
        object? details = null,
        CancellationToken ct = default);
}
