using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Oidc;
using QBEngineer.Core.Enums;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/oidc")]
[Authorize(Roles = "Admin")]
public class OidcAdminController(IMediator mediator) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string? GetIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

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
