using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QBEngineer.Api.Data;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record InitialSetupCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<LoginResponse>;

public class InitialSetupValidator : AbstractValidator<InitialSetupCommand>
{
    public InitialSetupValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
    }
}

public class InitialSetupHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<int>> roleManager,
    AppDbContext db,
    IConfiguration config)
    : IRequestHandler<InitialSetupCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(InitialSetupCommand request, CancellationToken cancellationToken)
    {
        if (userManager.Users.Any())
            throw new InvalidOperationException("Setup has already been completed.");

        // Create roles
        string[] roles = ["Admin", "Manager", "Engineer", "PM", "ProductionWorker", "OfficeManager"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }

        // Create admin user
        var initials = $"{request.FirstName[..1].ToUpper()}{request.LastName[..1].ToUpper()}";
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Initials = initials,
            AvatarColor = "#0d9488",
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create account: {errors}");
        }

        await userManager.AddToRoleAsync(user, "Admin");

        // Seed essential operational data
        await SeedData.SeedEssentialDataAsync(db);

        // Generate JWT
        var userRoles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, userRoles);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var userResponse = new AuthUserResponseModel(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.Initials, user.AvatarColor, userRoles.ToArray());

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
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
