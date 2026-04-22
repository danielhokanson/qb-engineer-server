using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Tests.Handlers.Oidc;

/// <summary>
/// In-memory audit recorder for OIDC handler tests. Captures every call so tests can
/// assert that a given event was logged without depending on the real EF sink.
/// </summary>
internal sealed class FakeOidcAuditService : IOidcAuditService
{
    public List<RecordedEvent> Events { get; } = new();

    public Task RecordAsync(
        OidcAuditEventType eventType,
        int? actorUserId = null,
        string? actorIp = null,
        string? clientId = null,
        int? ticketId = null,
        string? scopeName = null,
        object? details = null,
        CancellationToken ct = default)
    {
        Events.Add(new RecordedEvent(eventType, actorUserId, actorIp, clientId, ticketId, scopeName, details));
        return Task.CompletedTask;
    }

    public sealed record RecordedEvent(
        OidcAuditEventType EventType,
        int? ActorUserId,
        string? ActorIp,
        string? ClientId,
        int? TicketId,
        string? ScopeName,
        object? Details);
}
