using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record AuthUserResponseModel(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    string[] Roles,
    bool ProfileComplete);

// --- Login ---

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public record LoginResponse(string Token, DateTimeOffset ExpiresAt, AuthUserResponseModel User);

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

public class LoginHandler(UserManager<ApplicationUser> userManager, IConfiguration config, AppDbContext db)
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

        var expiresAt = DateTimeOffset.UtcNow.AddHours(24);

        var profileComplete = await CheckProfileComplete(user.Id, cancellationToken);

        var userResponse = new AuthUserResponseModel(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            roles.ToArray(),
            profileComplete);

        return new LoginResponse(token, expiresAt, userResponse);
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
            expires: DateTimeOffset.UtcNow.AddHours(24).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<bool> CheckProfileComplete(int userId, CancellationToken ct)
    {
        var profile = await db.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return false;

        return !string.IsNullOrWhiteSpace(profile.Street1) &&
               !string.IsNullOrWhiteSpace(profile.City) &&
               !string.IsNullOrWhiteSpace(profile.State) &&
               !string.IsNullOrWhiteSpace(profile.ZipCode) &&
               !string.IsNullOrWhiteSpace(profile.EmergencyContactName) &&
               !string.IsNullOrWhiteSpace(profile.EmergencyContactPhone) &&
               profile.W4CompletedAt is not null &&
               profile.I9CompletedAt is not null &&
               profile.StateWithholdingCompletedAt is not null &&
               profile.DirectDepositCompletedAt is not null &&
               profile.WorkersCompAcknowledgedAt is not null &&
               profile.HandbookAcknowledgedAt is not null;
    }
}

// --- Get Current User ---

public record GetCurrentUserQuery(ClaimsPrincipal User) : IRequest<AuthUserResponseModel>;

public class GetCurrentUserHandler(UserManager<ApplicationUser> userManager, AppDbContext db)
    : IRequestHandler<GetCurrentUserQuery, AuthUserResponseModel>
{
    public async Task<AuthUserResponseModel> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = request.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        var user = await userManager.FindByIdAsync(userId)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = await userManager.GetRolesAsync(user);

        var profile = await db.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);

        var profileComplete = profile is not null &&
            !string.IsNullOrWhiteSpace(profile.Street1) &&
            !string.IsNullOrWhiteSpace(profile.City) &&
            !string.IsNullOrWhiteSpace(profile.State) &&
            !string.IsNullOrWhiteSpace(profile.ZipCode) &&
            !string.IsNullOrWhiteSpace(profile.EmergencyContactName) &&
            !string.IsNullOrWhiteSpace(profile.EmergencyContactPhone) &&
            profile.W4CompletedAt is not null &&
            profile.I9CompletedAt is not null &&
            profile.StateWithholdingCompletedAt is not null &&
            profile.DirectDepositCompletedAt is not null &&
            profile.WorkersCompAcknowledgedAt is not null &&
            profile.HandbookAcknowledgedAt is not null;

        return new AuthUserResponseModel(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Initials,
            user.AvatarColor,
            roles.ToArray(),
            profileComplete);
    }
}
