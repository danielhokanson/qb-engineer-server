using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Auth;

public record NfcKioskLoginCommand(string IdentifierValue, string Pin) : IRequest<LoginResponse>;

public class NfcKioskLoginValidator : AbstractValidator<NfcKioskLoginCommand>
{
    public NfcKioskLoginValidator()
    {
        RuleFor(x => x.IdentifierValue).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Pin).NotEmpty().Matches(@"^\d{4,8}$");
    }
}

public class NfcKioskLoginHandler(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : IRequestHandler<NfcKioskLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(NfcKioskLoginCommand request, CancellationToken cancellationToken)
    {
        var identifier = await db.UserScanIdentifiers
            .FirstOrDefaultAsync(x => x.IdentifierValue == request.IdentifierValue
                && x.IsActive, cancellationToken);

        if (identifier == null)
            throw new InvalidOperationException("Invalid scan identifier or PIN");

        var user = await userManager.FindByIdAsync(identifier.UserId.ToString());

        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Invalid scan identifier or PIN");

        if (string.IsNullOrEmpty(user.PinHash))
            throw new InvalidOperationException("PIN not configured. Contact admin.");

        if (!SetPinHandler.VerifyPin(request.Pin, user.PinHash))
            throw new InvalidOperationException("Invalid scan identifier or PIN");

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return new LoginResponse(
            token,
            DateTime.UtcNow.AddHours(8),
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
            new("authTier", "nfc"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
