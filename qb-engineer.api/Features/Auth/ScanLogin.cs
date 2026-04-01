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

/// <summary>
/// Unified scan authentication — accepts any scan identifier (RFID, NFC, barcode, biometric)
/// and resolves the user by checking UserScanIdentifiers first, then EmployeeBarcode fallback.
/// All scan types are handled by this single endpoint in parallel on the kiosk.
/// </summary>
public record ScanLoginCommand(string ScanValue, string Pin) : IRequest<LoginResponse>;

public class ScanLoginValidator : AbstractValidator<ScanLoginCommand>
{
    public ScanLoginValidator()
    {
        RuleFor(x => x.ScanValue).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Pin).NotEmpty().Matches(@"^\d{4,8}$")
            .WithMessage("PIN must be 4-8 digits.");
    }
}

public class ScanLoginHandler(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IConfiguration config) : IRequestHandler<ScanLoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(ScanLoginCommand request, CancellationToken cancellationToken)
    {
        ApplicationUser? user = null;
        string authTier = "scan";

        // 1. Check UserScanIdentifiers table (RFID, NFC, barcode, biometric)
        var identifier = await db.UserScanIdentifiers
            .FirstOrDefaultAsync(x => x.IdentifierValue == request.ScanValue
                && x.IsActive, cancellationToken);

        if (identifier != null)
        {
            user = await userManager.FindByIdAsync(identifier.UserId.ToString());
            authTier = identifier.IdentifierType; // "rfid", "nfc", "barcode", "biometric"
        }

        // 2. Fallback: check ApplicationUser.EmployeeBarcode field
        if (user == null)
        {
            user = await userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeBarcode == request.ScanValue
                    && u.IsActive, cancellationToken);
            authTier = "barcode";
        }

        // 3. Validate user found and active
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Invalid scan identifier or PIN.");

        // 4. Verify PIN
        if (string.IsNullOrEmpty(user.PinHash))
            throw new InvalidOperationException("PIN not configured. Contact your administrator.");

        if (!SetPinHandler.VerifyPin(request.Pin, user.PinHash))
            throw new InvalidOperationException("Invalid scan identifier or PIN.");

        // 5. Generate JWT with auth tier claim
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwt(user, roles, authTier);

        return new LoginResponse(
            token,
            DateTimeOffset.UtcNow.AddHours(8),
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray(), false));
    }

    private string GenerateJwt(ApplicationUser user, IList<string> roles, string authTier)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("authTier", authTier),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTimeOffset.UtcNow.AddHours(8).UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
