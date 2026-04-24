using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/oidc")]
[Authorize(Roles = "Admin")]
public class OidcAdminController(IMediator mediator, IOidcProviderSettings providerSettings) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    // --- Provider settings (feature toggle + base URL) ---

    public record ProviderSettingsResponse(bool ProviderEnabled, string PublicBaseUrl);
    public record UpdateProviderSettingsRequest(bool ProviderEnabled, string PublicBaseUrl);

    /// <summary>
    /// Returns the current runtime OIDC provider settings (DB-resolved, falling back to
    /// appsettings). Surfaced in the admin UI so operators can flip the provider on/off
    /// and set the public base URL without redeploying.
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<ProviderSettingsResponse>> GetSettings()
    {
        var snap = await providerSettings.GetAsync(HttpContext.RequestAborted);
        return Ok(new ProviderSettingsResponse(snap.ProviderEnabled, snap.PublicBaseUrl));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<ProviderSettingsResponse>> UpdateSettings([FromBody] UpdateProviderSettingsRequest body)
    {
        await providerSettings.UpdateAsync(body.ProviderEnabled, body.PublicBaseUrl ?? string.Empty, HttpContext.RequestAborted);
        return Ok(new ProviderSettingsResponse(body.ProviderEnabled, body.PublicBaseUrl ?? string.Empty));
    }

    // --- Registration tickets ---

    [HttpGet("tickets")]
    public async Task<ActionResult<IReadOnlyList<TicketListItem>>> ListTickets([FromQuery] OidcTicketStatus? status)
        => Ok(await mediator.Send(new ListTicketsQuery(status)));

    public record MintTicketRequest(
        string ExpectedClientName,
        string AllowedRedirectUriPrefix,
        string? AllowedPostLogoutRedirectUriPrefix,
        List<string> AllowedScopes,
        List<string>? RequiredRolesForUsers,
        int TtlHours,
        bool RequireSignedSoftwareStatement,
        List<string>? TrustedPublisherKeyIds,
        string? Notes);

    [HttpPost("tickets")]
    public async Task<ActionResult<MintTicketResponse>> MintTicket([FromBody] MintTicketRequest body)
    {
        var result = await mediator.Send(new MintTicketCommand(
            body.ExpectedClientName,
            body.AllowedRedirectUriPrefix,
            body.AllowedPostLogoutRedirectUriPrefix,
            body.AllowedScopes,
            body.RequiredRolesForUsers,
            body.TtlHours,
            body.RequireSignedSoftwareStatement,
            body.TrustedPublisherKeyIds,
            body.Notes,
            GetUserId(),
            GetIp()));
        return CreatedAtAction(nameof(ListTickets), new { status = OidcTicketStatus.Issued }, result);
    }

    [HttpDelete("tickets/{id:int}")]
    public async Task<IActionResult> RevokeTicket(int id, [FromQuery] string? reason)
    {
        await mediator.Send(new RevokeTicketCommand(id, GetUserId(), GetIp(), reason));
        return NoContent();
    }

    // --- Clients ---

    [HttpGet("clients")]
    public async Task<ActionResult<IReadOnlyList<ClientListItem>>> ListClients([FromQuery] OidcClientStatus? status)
        => Ok(await mediator.Send(new ListClientsQuery(status)));

    [HttpGet("clients/{clientId}")]
    public async Task<ActionResult<ClientDetailResponse>> GetClient(string clientId)
        => Ok(await mediator.Send(new GetClientQuery(clientId)));

    public record ProvisionClientRequest(
        string ClientName,
        List<string> RedirectUris,
        List<string>? PostLogoutRedirectUris,
        List<string> Scopes,
        bool ApproveImmediately,
        bool IsFirstParty,
        bool RequireConsent,
        string? RequiredRolesCsv,
        string? OwnerEmail,
        string? Description,
        string? Notes);

    /// <summary>
    /// Direct admin provisioning — creates an OpenIddict client + metadata in one call and returns
    /// the client_id / client_secret / registration_access_token inline. Bypasses the RFC 7591
    /// ticket ceremony for apps the admin owns themselves.
    /// </summary>
    [HttpPost("clients")]
    public async Task<ActionResult<ProvisionClientResponse>> ProvisionClient([FromBody] ProvisionClientRequest body)
    {
        var result = await mediator.Send(new ProvisionClientCommand(
            body.ClientName,
            body.RedirectUris,
            body.PostLogoutRedirectUris,
            body.Scopes,
            body.ApproveImmediately,
            body.IsFirstParty,
            body.RequireConsent,
            body.RequiredRolesCsv,
            body.OwnerEmail,
            body.Description,
            body.Notes,
            GetUserId(),
            GetIp()));
        return CreatedAtAction(nameof(GetClient), new { clientId = result.ClientId }, result);
    }

    public record ApproveClientRequest(
        bool IsFirstParty,
        bool RequireConsent,
        string? AllowedCustomScopesCsv,
        string? RequiredRolesCsv,
        string? Notes);

    [HttpPost("clients/{clientId}/approve")]
    public async Task<IActionResult> Approve(string clientId, [FromBody] ApproveClientRequest body)
    {
        await mediator.Send(new ApproveClientCommand(
            clientId, GetUserId(), GetIp(),
            body.IsFirstParty, body.RequireConsent,
            body.AllowedCustomScopesCsv, body.RequiredRolesCsv, body.Notes));
        return NoContent();
    }

    public record SuspendClientRequest(string? Reason);

    [HttpPost("clients/{clientId}/suspend")]
    public async Task<IActionResult> Suspend(string clientId, [FromBody] SuspendClientRequest body)
    {
        await mediator.Send(new SuspendClientCommand(clientId, GetUserId(), GetIp(), body.Reason));
        return NoContent();
    }

    public record RevokeClientRequest(string? Reason);

    [HttpDelete("clients/{clientId}")]
    public async Task<IActionResult> Revoke(string clientId, [FromQuery] string? reason)
    {
        await mediator.Send(new RevokeClientCommand(clientId, GetUserId(), GetIp(), reason));
        return NoContent();
    }

    [HttpPost("clients/{clientId}/rotate-secret")]
    public async Task<ActionResult<RotateSecretResponse>> RotateSecret(string clientId)
        => Ok(await mediator.Send(new RotateSecretCommand(clientId, GetUserId(), GetIp())));

    public record UpdateClientRequest(
        bool RequireConsent,
        bool IsFirstParty,
        string? RequiredRolesCsv,
        string? AllowedCustomScopesCsv,
        string? Description,
        string? OwnerEmail,
        string? Notes);

    [HttpPatch("clients/{clientId}")]
    public async Task<IActionResult> Update(string clientId, [FromBody] UpdateClientRequest body)
    {
        await mediator.Send(new UpdateClientCommand(
            clientId, GetUserId(), GetIp(),
            body.RequireConsent, body.IsFirstParty,
            body.RequiredRolesCsv, body.AllowedCustomScopesCsv,
            body.Description, body.OwnerEmail, body.Notes));
        return NoContent();
    }

    // --- Custom scopes ---

    [HttpGet("scopes")]
    public async Task<ActionResult<IReadOnlyList<ScopeListItem>>> ListScopes([FromQuery] bool includeInactive = false)
        => Ok(await mediator.Send(new ListScopesQuery(includeInactive)));

    public record CreateScopeRequest(
        string Name,
        string DisplayName,
        string Description,
        string ClaimMappingsJson,
        string? ResourcesCsv);

    [HttpPost("scopes")]
    public async Task<ActionResult<int>> CreateScope([FromBody] CreateScopeRequest body)
    {
        var id = await mediator.Send(new CreateScopeCommand(
            body.Name, body.DisplayName, body.Description,
            body.ClaimMappingsJson, body.ResourcesCsv,
            GetUserId(), GetIp()));
        return CreatedAtAction(nameof(ListScopes), new { id }, new { id });
    }

    public record UpdateScopeRequest(
        string DisplayName,
        string Description,
        string ClaimMappingsJson,
        string? ResourcesCsv,
        bool IsActive);

    [HttpPatch("scopes/{id:int}")]
    public async Task<IActionResult> UpdateScope(int id, [FromBody] UpdateScopeRequest body)
    {
        await mediator.Send(new UpdateScopeCommand(
            id, body.DisplayName, body.Description,
            body.ClaimMappingsJson, body.ResourcesCsv, body.IsActive,
            GetUserId(), GetIp()));
        return NoContent();
    }

    [HttpDelete("scopes/{id:int}")]
    public async Task<IActionResult> DeleteScope(int id)
    {
        await mediator.Send(new DeleteScopeCommand(id, GetUserId(), GetIp()));
        return NoContent();
    }

    // --- Audit ---

    [HttpGet("audit")]
    public async Task<ActionResult<IReadOnlyList<AuditEventListItem>>> ListAudit(
        [FromQuery] OidcAuditEventType? eventType,
        [FromQuery] string? clientId,
        [FromQuery] int? ticketId,
        [FromQuery] int? actorUserId,
        [FromQuery] DateTimeOffset? since,
        [FromQuery] DateTimeOffset? until,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
        => Ok(await mediator.Send(new ListAuditEventsQuery(
            eventType, clientId, ticketId, actorUserId, since, until, skip, take)));
}
