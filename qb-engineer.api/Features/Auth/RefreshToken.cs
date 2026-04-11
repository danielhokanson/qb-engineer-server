using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Identity;

using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record RefreshTokenCommand(string CurrentJti, int UserId) : IRequest<LoginResponse>;

public class RefreshTokenHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ISessionStore sessionStore) : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Verify user still exists and is active
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new UnauthorizedAccessException("User not found");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is disabled");

        // Generate new token
        var roles = await userManager.GetRolesAsync(user);
        var result = tokenService.GenerateToken(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.Initials, user.AvatarColor, roles);

        // Rotate session JTI (atomically replaces old JTI with new one)
        var updated = await sessionStore.UpdateSessionJtiAsync(
            request.CurrentJti, result.Jti, result.ExpiresAt, cancellationToken);

        if (updated is null)
            throw new UnauthorizedAccessException("Session is no longer valid");

        return new LoginResponse(
            result.Token,
            result.ExpiresAt,
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray(), false));
    }
}
