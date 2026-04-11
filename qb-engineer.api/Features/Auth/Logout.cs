using System.IdentityModel.Tokens.Jwt;

using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Auth;

public record LogoutCommand(string? Jti) : IRequest;

public class LogoutHandler(ISessionStore sessionStore) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.Jti))
        {
            await sessionStore.RevokeSessionAsync(request.Jti, cancellationToken);
        }
    }
}
