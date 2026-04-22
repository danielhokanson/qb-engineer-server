using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Endpoints consumed by the Angular consent screen during a <c>/connect/authorize</c> passthrough.
/// <list type="bullet">
///   <item><c>GET /api/v1/oidc/consent/context</c> — details the consent screen renders.</item>
///   <item><c>POST /api/v1/oidc/consent/grant</c> — records Allow; creates a permanent authorization.</item>
///   <item><c>POST /api/v1/oidc/consent/deny</c> — records Deny; audited only.</item>
/// </list>
/// Authenticated via the standard JWT bearer scheme — the SPA must already be logged in to qb-engineer.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/oidc/consent")]
public class OidcConsentController(IMediator mediator, IOptions<OidcOptions> oidcOptions) : ControllerBase
{
    private readonly OidcOptions _oidcOptions = oidcOptions.Value;

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet("context")]
    public async Task<IActionResult> GetContext(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "scope")] string? scope)
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return BadRequest(new { error = "client_id is required." });
        }

        var requestedScopes = (scope ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        try
        {
            var result = await mediator.Send(new GetConsentContextQuery(clientId, requestedScopes));
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("grant")]
    public async Task<IActionResult> Grant([FromBody] ConsentRequest body)
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();
        if (body is null || string.IsNullOrWhiteSpace(body.ClientId))
        {
            return BadRequest(new { error = "clientId is required." });
        }

        try
        {
            await mediator.Send(new GrantConsentCommand(
                GetUserId(),
                body.ClientId,
                body.Scopes ?? Array.Empty<string>(),
                GetIp()));
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("deny")]
    public async Task<IActionResult> Deny([FromBody] ConsentRequest body)
    {
        if (!_oidcOptions.ProviderEnabled) return NotFound();
        if (body is null || string.IsNullOrWhiteSpace(body.ClientId))
        {
            return BadRequest(new { error = "clientId is required." });
        }

        await mediator.Send(new DenyConsentCommand(
            GetUserId(),
            body.ClientId,
            body.Scopes ?? Array.Empty<string>(),
            GetIp()));
        return NoContent();
    }
}

public class ConsentRequest
{
    public string ClientId { get; set; } = string.Empty;
    public IReadOnlyList<string>? Scopes { get; set; }
}
