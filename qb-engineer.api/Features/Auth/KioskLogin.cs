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

public record KioskLoginCommand(string Barcode, string Pin) : IRequest<LoginResponse>;

public class KioskLoginValidator : AbstractValidator<KioskLoginCommand>
{
    public KioskLoginValidator()
    {
        RuleFor(x => x.Barcode).NotEmpty();
        RuleFor(x => x.Pin).NotEmpty();
    }
}

public class KioskLoginHandler(
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : IRequestHandler<KioskLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(KioskLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.EmployeeBarcode == request.Barcode && u.IsActive, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.PinHash))
            throw new InvalidOperationException("Invalid barcode or PIN");

        if (!SetPinHandler.VerifyPin(request.Pin, user.PinHash))
            throw new InvalidOperationException("Invalid barcode or PIN");

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles);

        return new LoginResponse(
            token,
            DateTime.UtcNow.AddHours(8),
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray(), false));
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
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
