using System.Collections.Immutable;
using System.Security.Claims;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// OpenIddict passthrough endpoints for qb-engineer's OIDC provider surface.
/// <list type="bullet">
///   <item><c>GET /connect/authorize</c> — interactive authorization code issuance with role gating.</item>
///   <item><c>POST /connect/token</c> — authorization code / refresh token exchange.</item>
///   <item><c>GET /connect/userinfo</c> — emits claims for the bearer access token.</item>
///   <item><c>POST /connect/logout</c> — end-session request.</item>
///   <item><c>POST /api/v1/oidc/interactive-login</c> — SPA bridge: exchanges a JWT principal for the OidcInteractive cookie.</item>
/// </list>
/// The interactive cookie is a short-lived browser credential used <b>only</b> for the /connect/authorize
/// redirect. Real API access still flows through the JWT bearer scheme.
/// </summary>
[ApiController]
public class OidcAuthorizationController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IOptions<OidcOptions> oidcOptions,
    IOidcAuditService audit,
    IOpenIddictApplicationManager appManager,
    IOpenIddictAuthorizationManager authManager) : ControllerBase
{
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    // ─────────────────────────────────────────────────────────────────────────
    // SPA bridge: convert JWT → OidcInteractive cookie
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called from the Angular SPA once the user has logged in with the normal JWT flow.
    /// Issues a short-lived cookie the <c>/connect/authorize</c> endpoint can use to discover
    /// the current user during an OIDC passthrough redirect. The JWT principal is re-emitted
    /// as-is into the cookie scheme.
    /// </summary>
    [HttpPost("/api/v1/oidc/interactive-login")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> InteractiveLogin()
    {
        if (!_oidcOptions.ProviderEnabled)
        {
            return StatusCode(503, new { error = "provider_disabled" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        var identity = new ClaimsIdentity("OidcInteractive");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? userId));
        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        }
        foreach (var role in await userManager.GetRolesAsync(user))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        await HttpContext.SignInAsync("OidcInteractive", new ClaimsPrincipal(identity));
        return NoContent();
    }

    [HttpPost("/api/v1/oidc/interactive-logout")]
    [AllowAnonymous]
    public async Task<IActionResult> InteractiveLogout()
    {
        await HttpContext.SignOutAsync("OidcInteractive");
        return NoContent();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /connect/authorize
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("/connect/authorize")]
    [HttpPost("/connect/authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> Authorize()
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();

        var req = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict server request could not be retrieved.");

        // 1. Require an authenticated interactive session. If absent, challenge the OidcInteractive
        //    scheme — the cookie handler's LoginPath (/oidc/login) will redirect the browser to
        //    the Angular login route with the full authorize querystring preserved.
        var cookieAuth = await HttpContext.AuthenticateAsync("OidcInteractive");
        if (cookieAuth?.Principal is null || cookieAuth.Principal.Identity?.IsAuthenticated != true)
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = Request.GetEncodedPathAndQuery() },
                "OidcInteractive");
        }

        var principal = cookieAuth.Principal;
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ForbidOidc(OpenIddictConstants.Errors.LoginRequired, "User identity could not be resolved.");
        }

        // 2. Resolve the client — must be Active.
        var metadata = await db.OidcClientMetadata.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ClientId == req.ClientId);
        if (metadata is null)
        {
            return ForbidOidc(OpenIddictConstants.Errors.InvalidClient, "Unknown client.");
        }

        if (metadata.Status != OidcClientStatus.Active)
        {
            await audit.RecordAsync(
                OidcAuditEventType.RoleGateDenied,
                actorIp: GetIp(),
                clientId: req.ClientId,
                details: new { reason = "client_not_active", status = metadata.Status.ToString() });
            return ForbidOidc(
                OpenIddictConstants.Errors.InvalidClient,
                metadata.Status == OidcClientStatus.Pending
                    ? "This client is pending administrator approval."
                    : "This client is not currently allowed to sign users in.");
        }

        // 3. Role gate.
        var userRoles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var requiredRoles = (metadata.RequiredRolesCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
        if (requiredRoles.Count > 0 && !requiredRoles.Any(r => userRoles.Contains(r, StringComparer.Ordinal)))
        {
            await audit.RecordAsync(
                OidcAuditEventType.RoleGateDenied,
                actorUserId: int.Parse(userId),
                actorIp: GetIp(),
                clientId: req.ClientId,
                details: new { requiredRoles, userRoles });
            return ForbidOidc(
                OpenIddictConstants.Errors.AccessDenied,
                "Your account does not have a role permitted to sign in to this application.");
        }

        // 4. Scope validation — every requested scope must either be a system scope or listed
        //    in the client's AllowedCustomScopesCsv.
        var requestedScopes = req.GetScopes();
        var allowedCustom = (metadata.AllowedCustomScopesCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
        var systemScopes = new HashSet<string>(StringComparer.Ordinal)
        {
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            OpenIddictConstants.Scopes.OfflineAccess,
            "roles",
        };
        foreach (var s in requestedScopes)
        {
            if (!systemScopes.Contains(s) && !allowedCustom.Contains(s))
            {
                await audit.RecordAsync(
                    OidcAuditEventType.ScopeDenied,
                    actorUserId: int.Parse(userId),
                    actorIp: GetIp(),
                    clientId: req.ClientId,
                    scopeName: s);
                return ForbidOidc(
                    OpenIddictConstants.Errors.InvalidScope,
                    $"Scope '{s}' is not permitted for this client.");
            }
        }

        // 5. Consent check — locate (or require) a permanent OpenIddict authorization that covers
        //    every requested scope. If RequireConsent is set and no such authorization exists, the
        //    browser is bounced to the Angular consent screen (/oidc/consent), which calls
        //    /api/v1/oidc/consent/grant to write the authorization, then redirects back to
        //    /connect/authorize. Clients with RequireConsent = false auto-grant without prompting.
        var app = await appManager.FindByClientIdAsync(req.ClientId!);
        var appId = app is null ? null : await appManager.GetIdAsync(app);
        if (string.IsNullOrWhiteSpace(appId))
        {
            return ForbidOidc(OpenIddictConstants.Errors.InvalidClient, "Client is not registered with the token server.");
        }

        var scopeArray = requestedScopes.ToImmutableArray();
        string? authorizationId = null;

        var existing = new List<object>();
        await foreach (var auth in authManager.FindAsync(
            subject: userId,
            client: appId,
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: scopeArray))
        {
            existing.Add(auth);
        }

        if (existing.Count > 0)
        {
            authorizationId = await authManager.GetIdAsync(existing[0]);
        }
        else if (metadata.RequireConsent)
        {
            var returnUrl = Uri.EscapeDataString(Request.GetEncodedPathAndQuery());
            var scopeParam = Uri.EscapeDataString(string.Join(' ', requestedScopes));
            var clientParam = Uri.EscapeDataString(req.ClientId!);
            return Redirect($"/oidc/consent?client_id={clientParam}&scope={scopeParam}&return_url={returnUrl}");
        }
        else
        {
            // Auto-grant path: persist a permanent authorization so subsequent calls skip lookup-only.
            var consentIdentity = new ClaimsIdentity("OidcConsent");
            consentIdentity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, userId));
            var consentPrincipal = new ClaimsPrincipal(consentIdentity);
            var created = await authManager.CreateAsync(
                principal: consentPrincipal,
                subject: userId,
                client: appId,
                type: OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes: scopeArray);
            authorizationId = await authManager.GetIdAsync(created);

            await audit.RecordAsync(
                OidcAuditEventType.ConsentGranted,
                actorUserId: int.Parse(userId),
                actorIp: GetIp(),
                clientId: req.ClientId,
                details: new { scopes = requestedScopes, autoGranted = true, isFirstParty = metadata.IsFirstParty });
        }

        // 6. Build the signed-in principal for OpenIddict.
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, userId);
        var name = principal.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Name, name);
            identity.AddClaim(OpenIddictConstants.Claims.PreferredUsername, name);
        }
        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            identity.AddClaim(OpenIddictConstants.Claims.Email, email);
        }
        foreach (var role in userRoles)
        {
            identity.AddClaim(OpenIddictConstants.Claims.Role, role);
        }

        // Apply custom scope → claim mappings for each active custom scope in the request.
        if (requestedScopes.Any(s => !systemScopes.Contains(s)))
        {
            var customScopes = await db.OidcCustomScopes
                .AsNoTracking()
                .Where(s => s.IsActive && requestedScopes.Contains(s.Name))
                .ToListAsync();
            foreach (var scope in customScopes)
            {
                foreach (var claim in ClaimMapper.ApplyMappings(scope.ClaimMappingsJson, principal, userRoles))
                {
                    identity.AddClaim(claim);
                }
            }
        }

        // Mark every claim with destinations so OpenIddict knows where to emit it.
        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(GetDestinations(claim, requestedScopes));
        }

        var oidcPrincipal = new ClaimsPrincipal(identity);
        oidcPrincipal.SetScopes(requestedScopes);
        if (!string.IsNullOrWhiteSpace(authorizationId))
        {
            oidcPrincipal.SetAuthorizationId(authorizationId);
        }

        return SignIn(oidcPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /connect/token
    // ─────────────────────────────────────────────────────────────────────────

    [HttpPost("/connect/token")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token()
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();

        var req = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict server request could not be retrieved.");

        if (req.IsAuthorizationCodeGrantType() || req.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result?.Principal is null)
            {
                return ForbidOidc(
                    OpenIddictConstants.Errors.InvalidGrant,
                    "The supplied authorization code or refresh token is invalid.");
            }

            // Re-check that the client is still Active and the user still exists.
            var clientId = req.ClientId ?? string.Empty;
            var metadata = await db.OidcClientMetadata.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ClientId == clientId);
            if (metadata is null || metadata.Status != OidcClientStatus.Active)
            {
                return ForbidOidc(
                    OpenIddictConstants.Errors.InvalidGrant,
                    "The client is no longer authorized to issue tokens.");
            }

            var userId = result.Principal.FindFirstValue(OpenIddictConstants.Claims.Subject);
            if (string.IsNullOrWhiteSpace(userId) || await userManager.FindByIdAsync(userId) is null)
            {
                return ForbidOidc(
                    OpenIddictConstants.Errors.InvalidGrant,
                    "The user account no longer exists.");
            }

            // Re-apply destinations (refresh token principals don't persist destinations).
            foreach (var claim in result.Principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, result.Principal.GetScopes()));
            }

            // Update last-used marker for admin visibility.
            var trackedMetadata = await db.OidcClientMetadata.FirstOrDefaultAsync(m => m.ClientId == clientId);
            if (trackedMetadata is not null)
            {
                trackedMetadata.LastUsedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }

            await audit.RecordAsync(
                OidcAuditEventType.TokenIssued,
                actorIp: GetIp(),
                clientId: clientId,
                details: new { grantType = req.GrantType });

            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return ForbidOidc(
            OpenIddictConstants.Errors.UnsupportedGrantType,
            "Only authorization_code and refresh_token grants are supported.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /connect/userinfo
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("/connect/userinfo")]
    [HttpPost("/connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserInfo()
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();

        var userId = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = userId,
        };

        if (User.HasScope(OpenIddictConstants.Scopes.Profile))
        {
            if (!string.IsNullOrWhiteSpace(user.UserName))
            {
                claims[OpenIddictConstants.Claims.PreferredUsername] = user.UserName;
                claims[OpenIddictConstants.Claims.Name] = user.UserName;
            }
        }

        if (User.HasScope(OpenIddictConstants.Scopes.Email) && !string.IsNullOrWhiteSpace(user.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = user.Email;
            claims[OpenIddictConstants.Claims.EmailVerified] = user.EmailConfirmed;
        }

        if (User.HasScope("roles"))
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Count > 0)
            {
                claims[OpenIddictConstants.Claims.Role] = roles.ToArray();
            }
        }

        // Forward custom-scope claims that the access token already carries.
        foreach (var claim in User.Claims)
        {
            if (claims.ContainsKey(claim.Type)) continue;
            if (claim.Type is OpenIddictConstants.Claims.Subject
                or OpenIddictConstants.Claims.Audience
                or OpenIddictConstants.Claims.Issuer
                or OpenIddictConstants.Claims.ExpiresAt
                or OpenIddictConstants.Claims.IssuedAt
                or OpenIddictConstants.Claims.NotBefore
                or OpenIddictConstants.Claims.JwtId
                or OpenIddictConstants.Claims.TokenUsage
                or OpenIddictConstants.Claims.Scope)
            {
                continue;
            }
            claims[claim.Type] = claim.Value;
        }

        return Ok(claims);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // /connect/logout
    // ─────────────────────────────────────────────────────────────────────────

    [HttpGet("/connect/logout")]
    [HttpPost("/connect/logout")]
    [AllowAnonymous]
    public async Task<IActionResult> EndSession()
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();

        await HttpContext.SignOutAsync("OidcInteractive");
        return SignOut(
            authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme });
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IActionResult ForbidOidc(string error, string description)
    {
        var props = new AuthenticationProperties(new Dictionary<string, string?>
        {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
        });
        return Forbid(props, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim, IEnumerable<string> scopes)
    {
        var scopeSet = scopes as HashSet<string> ?? new HashSet<string>(scopes, StringComparer.Ordinal);

        // Every claim lands in the access token.
        yield return OpenIddictConstants.Destinations.AccessToken;

        // Some claims also land in the ID token when the matching scope is granted.
        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name:
            case OpenIddictConstants.Claims.PreferredUsername:
                if (scopeSet.Contains(OpenIddictConstants.Scopes.Profile))
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                break;

            case OpenIddictConstants.Claims.Email:
            case OpenIddictConstants.Claims.EmailVerified:
                if (scopeSet.Contains(OpenIddictConstants.Scopes.Email))
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                break;

            case OpenIddictConstants.Claims.Role:
                if (scopeSet.Contains("roles"))
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                break;
        }
    }
}
