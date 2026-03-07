using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

// --- DTOs ---

public record AuthUserDto(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    string[] Roles);

// --- Login ---

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public record LoginResponse(string Token, DateTime ExpiresAt, AuthUserDto User);

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginHandler(UserManager<ApplicationUser> userManager, IConfiguration config)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = await userManager.GetRolesAsync(user);

        var token = GenerateJwtToken(user, roles);

        var expiresAt = DateTime.UtcNow.AddHours(24);

        var userDto = new AuthUserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            roles.ToArray());

        return new LoginResponse(token, expiresAt, userDto);
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
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

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "dev-secret-key-change-in-production-min-32-chars!!"));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "qb-engineer",
            audience: config["Jwt:Audience"] ?? "qb-engineer-ui",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// --- Get Current User ---

public record GetCurrentUserQuery(ClaimsPrincipal User) : IRequest<AuthUserDto>;

public class GetCurrentUserHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetCurrentUserQuery, AuthUserDto>
{
    public async Task<AuthUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = request.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = await userManager.GetRolesAsync(user);

        return new AuthUserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            roles.ToArray());
    }
}
