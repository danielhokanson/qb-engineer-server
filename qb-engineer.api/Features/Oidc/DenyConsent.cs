using MediatR;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Oidc;

/// <summary>
/// Records a user's rejection of a consent request. The Angular consent screen dispatches this command
/// when the user clicks Deny. No OpenIddict authorization is written — subsequent authorize calls will
/// prompt again. Used for audit visibility only.
/// </summary>
public record DenyConsentCommand(
    int UserId,
    string ClientId,
    IReadOnlyCollection<string> Scopes,
    string? ActorIp) : IRequest<Unit>;

public class DenyConsentHandler(IOidcAuditService audit)
    : IRequestHandler<DenyConsentCommand, Unit>
{
    public async Task<Unit> Handle(DenyConsentCommand request, CancellationToken ct)
    {
        await audit.RecordAsync(
            OidcAuditEventType.ConsentDenied,
            actorUserId: request.UserId,
            actorIp: request.ActorIp,
            clientId: request.ClientId,
            details: new { scopes = request.Scopes },
            ct: ct);

        return Unit.Value;
    }
}
