using System.Text.Json;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Api.Capabilities;
using QBEngineer.Api.Features.Capabilities.Descriptor;
using QBEngineer.Api.Hubs;
using QBEngineer.Api.Services;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Capabilities.Toggle;

/// <summary>
/// Phase 4 Phase-B — Mutates the <c>Enabled</c> flag of a single capability,
/// refreshes the <see cref="ICapabilitySnapshotProvider"/>, writes a system
/// audit row, and broadcasts a SignalR <c>capabilityChanged</c> event so all
/// connected clients can reload the descriptor.
///
/// Phase B is the minimum viable mutation surface — Phase C adds optimistic
/// concurrency, dependency-cascade checks, and preset application.
/// </summary>
public record ToggleCapabilityCommand(string Code, bool Enabled)
    : IRequest<CapabilityDescriptorEntry>;

public class ToggleCapabilityValidator : AbstractValidator<ToggleCapabilityCommand>
{
    public ToggleCapabilityValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^CAP-[A-Z0-9-]+$")
            .WithMessage("Capability code must match the catalog format CAP-{AREA}-{NAME}.");
    }
}

public class ToggleCapabilityHandler(
    AppDbContext db,
    ICapabilitySnapshotProvider snapshots,
    ISystemAuditWriter auditWriter,
    IHubContext<NotificationHub> notificationHub)
    : IRequestHandler<ToggleCapabilityCommand, CapabilityDescriptorEntry>
{
    public async Task<CapabilityDescriptorEntry> Handle(
        ToggleCapabilityCommand request,
        CancellationToken cancellationToken)
    {
        var row = await db.Capabilities
            .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken)
            ?? throw new KeyNotFoundException($"Capability '{request.Code}' not found.");

        var fromState = row.Enabled;
        var toState = request.Enabled;

        // Idempotent: still emits an audit row + broadcasts so cross-tab
        // sessions can confirm the click landed. (Same posture as
        // BiApiKeyRevoked in Phase 3 / WU-04.)
        row.Enabled = toState;
        await db.SaveChangesAsync(cancellationToken);

        // Refresh the in-memory snapshot so the very next request reflects
        // the new state (the gate middleware reads this dictionary).
        await snapshots.RefreshAsync(cancellationToken);

        // System-wide audit row.
        var actorId = db.CurrentUserId ?? 0;
        var details = JsonSerializer.Serialize(new
        {
            code = row.Code,
            from = fromState,
            to = toState,
        });
        var action = toState ? CapabilityAuditEvents.Enabled : CapabilityAuditEvents.Disabled;
        await auditWriter.WriteAsync(
            action: action,
            userId: actorId,
            entityType: CapabilityAuditEvents.EntityType,
            entityId: row.Id,
            details: details,
            ct: cancellationToken);

        // Broadcast to ALL connected clients (4D §4.4 / 4D-decisions-log #3).
        // Clients receive `capabilityChanged` and call CapabilityService.load()
        // to refresh their local descriptor signal.
        await notificationHub.Clients.All.SendAsync(
            "capabilityChanged",
            new { capabilityId = row.Code, enabled = toState },
            cancellationToken);

        return new CapabilityDescriptorEntry(
            Id: row.Code,
            Code: row.Code,
            Area: row.Area,
            Name: row.Name,
            Description: row.Description,
            Enabled: row.Enabled,
            IsDefaultOn: row.IsDefaultOn,
            RequiresRoles: row.RequiresRoles);
    }
}
