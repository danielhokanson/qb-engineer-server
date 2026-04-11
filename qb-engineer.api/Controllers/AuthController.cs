using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Auth;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserResponseModel>> Me()
    {
        var result = await mediator.Send(new GetCurrentUserQuery(User));
        return Ok(result);
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<SetupStatusResponseModel>> Status()
    {
        var result = await mediator.Send(new CheckSetupStatusQuery());
        return Ok(result);
    }

    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Setup(InitialSetupCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("validate-token/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<SetupTokenInfoResponse>> ValidateToken(string token)
    {
        var result = await mediator.Send(new ValidateSetupTokenQuery(token));
        return Ok(result);
    }

    [HttpPost("complete-setup")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> CompleteSetup(CompleteSetupCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("set-pin")]
    [Authorize]
    public async Task<IActionResult> SetPin(SetPinCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("kiosk-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> KioskLogin(KioskLoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("nfc-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> NfcLogin(NfcKioskLoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Unified scan login — accepts any scan identifier (RFID, NFC, barcode, biometric).
    /// Checks UserScanIdentifiers first, then falls back to EmployeeBarcode field.
    /// </summary>
    [HttpPost("scan-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> ScanLogin(ScanLoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<AuthUserResponseModel>> UpdateProfile(UpdateProfileCommand command)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(command with { UserId = userId });
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        await mediator.Send(new LogoutCommand(jti));
        return NoContent();
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Refresh()
    {
        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? throw new UnauthorizedAccessException("Missing token identifier");
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new RefreshTokenCommand(jti, userId));
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await mediator.Send(command with { UserId = userId });
        return NoContent();
    }

    [HttpGet("sso/providers")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSsoProviders()
        => Ok(await mediator.Send(new GetSsoProvidersQuery()));

    [HttpGet("sso/{provider}/login")]
    [AllowAnonymous]
    public IActionResult SsoLogin(string provider, [FromQuery] string? returnUrl)
    {
        var scheme = ResolveScheme(provider);
        var properties = new AuthenticationProperties
        {
            RedirectUri = $"/api/v1/auth/sso/{provider}/callback",
        };

        return Challenge(properties, scheme);
    }

    [HttpGet("sso/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> SsoCallback(string provider)
    {
        // Authenticate against the temporary cookie set during the OAuth round-trip
        var result = await HttpContext.AuthenticateAsync("SsoExternalCookie");

        if (!result.Succeeded)
            return Redirect("/sso/callback?error=sso_failed");

        var externalId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("No external ID found in SSO claims");
        var email = result.Principal?.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("No email found in SSO claims");

        try
        {
            var loginResponse = await mediator.Send(new SsoCallbackCommand(provider, externalId, email));

            // Clean up the temporary external cookie
            await HttpContext.SignOutAsync("SsoExternalCookie");

            return Redirect($"/sso/callback?sso_token={loginResponse.Token}");
        }
        catch (InvalidOperationException)
        {
            await HttpContext.SignOutAsync("SsoExternalCookie");
            return Redirect("/sso/callback?error=no_account");
        }
    }

    [HttpPost("sso/link")]
    [Authorize]
    public async Task<IActionResult> LinkSso([FromBody] LinkSsoIdentityCommand command)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await mediator.Send(command with { UserId = userId });
        return NoContent();
    }

    [HttpDelete("sso/unlink/{provider}")]
    [Authorize]
    public async Task<IActionResult> UnlinkSso(string provider)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await mediator.Send(new UnlinkSsoIdentityCommand(userId, provider));
        return NoContent();
    }

    [HttpGet("sso/linked")]
    [Authorize]
    public async Task<IActionResult> GetLinkedProviders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await mediator.Send(new GetLinkedSsoProvidersQuery(userId)));
    }

    private static string ResolveScheme(string provider) => provider switch
    {
        "google" => GoogleDefaults.AuthenticationScheme,
        "microsoft" => MicrosoftAccountDefaults.AuthenticationScheme,
        "oidc" => OpenIdConnectDefaults.AuthenticationScheme,
        _ => throw new InvalidOperationException($"Unknown SSO provider: {provider}")
    };
}
