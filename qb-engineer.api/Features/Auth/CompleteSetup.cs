using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QBEngineer.Api.Features.Auth;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record CompleteSetupCommand(
    string Token,
    string Password,
    string? FirstName,
    string? LastName) : IRequest<LoginResponse>;

public class CompleteSetupValidator : AbstractValidator<CompleteSetupCommand>
{
    public CompleteSetupValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class CompleteSetupHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : IRequestHandler<CompleteSetupCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(CompleteSetupCommand request, CancellationToken cancellationToken)
    {
        var normalizedToken = request.Token.Trim().ToUpperInvariant();
        var users = userManager.Users
            .Where(u => u.SetupToken == normalizedToken && u.SetupTokenExpiresAt > DateTime.UtcNow);

        var user = users.FirstOrDefault()
            ?? throw new InvalidOperationException("Invalid or expired setup token");

        // Set password
        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded && user.PasswordHash != null)
            throw new InvalidOperationException("Failed to reset password");

        var addResult = await userManager.AddPasswordAsync(user, request.Password);
        if (!addResult.Succeeded)
        {
            var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password requirements not met: {errors}");
        }

        // Update profile if provided
        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        // Clear setup token
        user.SetupToken = null;
        user.SetupTokenExpiresAt = null;
        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;

        await userManager.UpdateAsync(user);

        // Generate JWT and return login response
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return new LoginResponse(
            token,
            DateTime.UtcNow.AddHours(24),
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray()));
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
