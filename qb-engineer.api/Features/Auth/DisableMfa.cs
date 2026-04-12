using System.Security.Claims;

using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Auth;

public record DisableMfaCommand(ClaimsPrincipal User) : IRequest;

public class DisableMfaHandler(IMfaService mfaService) : IRequestHandler<DisableMfaCommand>
{
    public async Task Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var userId = int.Parse(request.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

        await mfaService.DisableMfaAsync(userId, cancellationToken);
    }
}
