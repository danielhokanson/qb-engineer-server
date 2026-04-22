using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>Transitions a <see cref="OidcClientStatus.Pending"/> client to <see cref="OidcClientStatus.Active"/>.</summary>
public record ApproveClientCommand(
    string ClientId,
    int ActorUserId,
    string? ActorIp,
    bool IsFirstParty,
    bool RequireConsent,
    string? AllowedCustomScopesCsv,
    string? RequiredRolesCsv,
    string? Notes) : IRequest<Unit>;

public class ApproveClientHandler(AppDbContext db, IClock clock, IOidcAuditService audit)
    : IRequestHandler<ApproveClientCommand, Unit>
{
    public async Task<Unit> Handle(ApproveClientCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        if (metadata.Status != OidcClientStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Client is {metadata.Status}; only Pending clients can be approved.");
        }

        metadata.Status = OidcClientStatus.Active;
        metadata.ApprovedByUserId = request.ActorUserId;
        metadata.ApprovedAt = clock.UtcNow;
        metadata.IsFirstParty = request.IsFirstParty;
        metadata.RequireConsent = request.RequireConsent;
        metadata.AllowedCustomScopesCsv = request.AllowedCustomScopesCsv;
        if (request.RequiredRolesCsv is not null)
        {
            metadata.RequiredRolesCsv = request.RequiredRolesCsv;
        }
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            metadata.Notes = request.Notes;
        }

        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ClientApproved,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new
            {
                request.IsFirstParty,
                request.RequireConsent,
                request.AllowedCustomScopesCsv,
                request.RequiredRolesCsv,
            },
            ct: ct);

        return Unit.Value;
    }
}
