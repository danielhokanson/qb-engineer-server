using MediatR;

using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Soft-deletes an admin-defined scope. System scopes (openid, profile, email, offline_access)
/// cannot be deleted. Clients that allow this scope continue to function — on the next token
/// request the scope is silently dropped from the issued claim set.
/// </summary>
public record DeleteScopeCommand(int Id, int ActorUserId, string? ActorIp) : IRequest<Unit>;

public class DeleteScopeHandler(AppDbContext db, IOidcAuditService audit)
    : IRequestHandler<DeleteScopeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteScopeCommand request, CancellationToken ct)
    {
        var scope = await db.OidcCustomScopes.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"OIDC scope {request.Id} not found.");

        if (scope.IsSystem)
        {
            throw new InvalidOperationException("System scopes cannot be deleted.");
        }

        scope.IsActive = false;
        scope.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.RecordAsync(
            OidcAuditEventType.ScopeDeleted,
            actorUserId: request.ActorUserId,
            actorIp: request.ActorIp,
            scopeName: scope.Name,
            ct: ct);

        return Unit.Value;
    }
}
