using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Admin edit of the sidecar metadata only (consent, first-party flag, role gate, allowed custom scopes,
/// description, owner email, notes). OpenIddict-owned fields (client_id, redirect URIs, permissions)
/// are NOT mutated here — those require a dedicated endpoint so admins think twice before touching them.
/// </summary>
public record UpdateClientCommand(
    string ClientId,
    int ActorUserId,
    string? ActorIp,
    bool RequireConsent,
    bool IsFirstParty,
    string? RequiredRolesCsv,
    string? AllowedCustomScopesCsv,
    string? Description,
    string? OwnerEmail,
    string? Notes) : IRequest<Unit>;

public class UpdateClientHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<UpdateClientCommand, Unit>
{
    public async Task<Unit> Handle(UpdateClientCommand request, CancellationToken ct)
    {
        var metadata = await db.OidcClientMetadata
            .FirstOrDefaultAsync(m => m.ClientId == request.ClientId, ct)
            ?? throw new KeyNotFoundException($"OIDC client '{request.ClientId}' not found.");

        metadata.RequireConsent = request.RequireConsent;
        metadata.IsFirstParty = request.IsFirstParty;
        metadata.RequiredRolesCsv = request.RequiredRolesCsv;
        metadata.AllowedCustomScopesCsv = request.AllowedCustomScopesCsv;
        metadata.Description = request.Description;
        metadata.OwnerEmail = request.OwnerEmail;
        metadata.Notes = request.Notes;

        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ClientUpdated,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new
            {
                request.RequireConsent,
                request.IsFirstParty,
                request.RequiredRolesCsv,
                request.AllowedCustomScopesCsv,
                request.Description,
                request.OwnerEmail,
            },
            ct: ct);

        return Unit.Value;
    }
}
