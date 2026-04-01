using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record SsoCallbackCommand(string Provider, string ExternalId, string Email) : IRequest<LoginResponse>;

public class SsoCallbackHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : IRequestHandler<SsoCallbackCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(SsoCallbackCommand request, CancellationToken cancellationToken)
    {
        // Find user by SSO identity link
        ApplicationUser? user = request.Provider switch
        {
            "google" => await userManager.Users.FirstOrDefaultAsync(
                u => u.GoogleId == request.ExternalId && u.IsActive, cancellationToken),
            "microsoft" => await userManager.Users.FirstOrDefaultAsync(
                u => u.MicrosoftId == request.ExternalId && u.IsActive, cancellationToken),
            "oidc" => await userManager.Users.FirstOrDefaultAsync(
                u => u.OidcSubjectId == request.ExternalId && u.IsActive, cancellationToken),
            _ => null
        };

        // If no linked account, try to find by email (auto-link on first SSO login)
        if (user == null)
        {
            user = await userManager.Users.FirstOrDefaultAsync(
                u => u.Email == request.Email && u.IsActive, cancellationToken);

            if (user == null)
                throw new InvalidOperationException("No account found. Contact your administrator to create an account first.");

            // Auto-link the SSO identity
            switch (request.Provider)
            {
                case "google": user.GoogleId = request.ExternalId; break;
                case "microsoft": user.MicrosoftId = request.ExternalId; break;
                case "oidc":
                    user.OidcSubjectId = request.ExternalId;
                    user.OidcProvider = request.Provider;
                    break;
            }

            await userManager.UpdateAsync(user);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return new LoginResponse(
            token,
            DateTimeOffset.UtcNow.AddHours(24),
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray(), false));
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
        };

        if (user.Initials is not null)
            claims.Add(new Claim("initials", user.Initials));

        if (user.AvatarColor is not null)
            claims.Add(new Claim("avatarColor", user.AvatarColor));

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "dev-secret-key-change-in-production-min-32-chars!!"));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "qb-engineer",
            audience: config["Jwt:Audience"] ?? "qb-engineer-ui",
            claims: claims,
            expires: DateTimeOffset.UtcNow.AddHours(24).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
