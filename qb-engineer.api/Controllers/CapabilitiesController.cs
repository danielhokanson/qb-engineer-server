using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using QBEngineer.Api.Capabilities;
using QBEngineer.Api.Features.Capabilities.BulkToggle;
using QBEngineer.Api.Features.Capabilities.Config;
using QBEngineer.Api.Features.Capabilities.Descriptor;
using QBEngineer.Api.Features.Capabilities.Toggle;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Phase 4 Phase-A — Read-only capability descriptor surface.
/// Phase 4 Phase-B — Adds the minimum viable toggle mutation.
/// Phase 4 Phase-C — Extends the mutation surface with optimistic concurrency
/// (If-Match / 412), dependency / mutex enforcement (409 with envelope), the
/// config endpoint, and the bulk-toggle endpoint. All three mutation routes
/// emit richer audit content (before/after, actor, optional reason).
///
/// Per Phase B D3/D4 (and Phase C reaffirmation): admin endpoints are gated
/// by the <c>Admin</c> role and carry <c>[CapabilityBootstrap]</c> so they
/// are NOT themselves capability-gated — disabling
/// <c>CAP-IDEN-CAPABILITY-ADMIN</c> would otherwise brick recovery.
/// </summary>
[ApiController]
[Route("api/v1/capabilities")]
[Authorize]
public class CapabilitiesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Phase 4 Phase-A — full capability descriptor for the installation.
    /// Phase B's UI calls this on login and on SignalR <c>capabilityChanged</c>.
    /// Phase C — each entry carries an <c>etag</c> the admin UI hands back as
    /// <c>If-Match</c> on subsequent toggle / config writes.
    /// </summary>
    [HttpGet("descriptor")]
    [CapabilityBootstrap]
    public async Task<ActionResult<CapabilityDescriptorResponseModel>> GetDescriptor()
    {
        var result = await mediator.Send(new GetCapabilityDescriptorQuery());
        return Ok(result);
    }

    /// <summary>
    /// Phase 4 Phase-B / Phase-C — toggles a single capability's enabled flag.
    ///
    /// Phase C semantics:
    ///   • <c>If-Match</c> header (weak ETag of the row's <c>Version</c>):
    ///     mismatch → 412 with WU-02 envelope.
    ///   • Disable when other enabled capabilities depend on this row →
    ///     409 with <c>capability-has-dependents</c>.
    ///   • Enable when required dependencies are disabled →
    ///     409 with <c>capability-missing-dependencies</c>.
    ///   • Enable when a soft-mutex peer is enabled →
    ///     409 with <c>capability-mutex-violation</c>.
    /// </summary>
    [HttpPut("{id}/enabled")]
    [Authorize(Roles = "Admin")]
    [CapabilityBootstrap]
    public async Task<ActionResult<CapabilityDescriptorEntry>> SetEnabled(
        string id,
        [FromBody] ToggleCapabilityRequestModel body,
        CancellationToken ct)
    {
        var ifMatch = Request.Headers[HeaderNames.IfMatch].ToString();
        try
        {
            var result = await mediator.Send(
                new ToggleCapabilityCommand(id, body.Enabled, ifMatch, body.Reason),
                ct);
            Response.Headers[HeaderNames.ETag] = result.ETag;
            return Ok(result);
        }
        catch (CapabilityMutationException ex)
        {
            return ToCapabilityMutationResult(ex);
        }
    }

    /// <summary>
    /// Phase 4 Phase-C — updates a capability's opaque config payload.
    /// Optimistic concurrency via <c>If-Match</c> against the
    /// CapabilityConfig row's separate Version (toggles and config edits
    /// each have their own ETag space).
    /// </summary>
    [HttpPut("{id}/config")]
    [Authorize(Roles = "Admin")]
    [CapabilityBootstrap]
    public async Task<ActionResult<CapabilityDescriptorEntry>> SetConfig(
        string id,
        [FromBody] UpdateCapabilityConfigRequestModel body,
        CancellationToken ct)
    {
        var ifMatch = Request.Headers[HeaderNames.IfMatch].ToString();
        try
        {
            var result = await mediator.Send(
                new UpdateCapabilityConfigCommand(id, body.ConfigJson, ifMatch, body.Reason),
                ct);
            if (!string.IsNullOrEmpty(result.ConfigETag))
                Response.Headers[HeaderNames.ETag] = result.ConfigETag;
            return Ok(result);
        }
        catch (CapabilityMutationException ex)
        {
            return ToCapabilityMutationResult(ex);
        }
    }

    /// <summary>
    /// Phase 4 Phase-C — atomic bulk toggle. Validates ALL dependency / mutex
    /// constraints across the WHOLE candidate state set before applying any
    /// change. Used as the substrate for Phase G's preset-apply.
    /// </summary>
    [HttpPost("bulk-toggle")]
    [Authorize(Roles = "Admin")]
    [CapabilityBootstrap]
    public async Task<ActionResult<IReadOnlyList<CapabilityDescriptorEntry>>> BulkToggle(
        [FromBody] BulkToggleCapabilitiesRequestModel body,
        CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new BulkToggleCapabilitiesCommand(body.Items, body.Reason),
                ct);
            return Ok(result);
        }
        catch (CapabilityMutationException ex)
        {
            return ToCapabilityMutationResult(ex);
        }
    }

    private ActionResult ToCapabilityMutationResult(CapabilityMutationException ex)
    {
        Response.ContentType = "application/problem+json";
        return new ObjectResult(ex.ToEnvelope())
        {
            StatusCode = ex.StatusCode,
        };
    }
}
