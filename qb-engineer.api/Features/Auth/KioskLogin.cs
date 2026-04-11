using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Interfaces;
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
    ITokenService tokenService,
    ISessionStore sessionStore,
    IHttpContextAccessor httpContext) : IRequestHandler<KioskLoginCommand, LoginResponse>
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
        var result = tokenService.GenerateToken(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.Initials, user.AvatarColor, roles,
            TimeSpan.FromHours(8));

        await sessionStore.CreateSessionAsync(user.Id, result.Jti, result.ExpiresAt,
            "kiosk",
            httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            httpContext.HttpContext?.Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return new LoginResponse(
            result.Token,
            result.ExpiresAt,
            new AuthUserResponseModel(
                user.Id, user.Email!, user.FirstName, user.LastName,
                user.Initials, user.AvatarColor, roles.ToArray(), false));
    }
}
