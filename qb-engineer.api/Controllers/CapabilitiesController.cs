using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Capabilities;
using QBEngineer.Api.Features.Capabilities.Descriptor;
using QBEngineer.Api.Features.Capabilities.Toggle;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Phase 4 Phase-A — Read-only capability descriptor surface.
/// Phase 4 Phase-B — Adds the minimum viable toggle mutation. Phase C will
/// flesh out config edits, presets, optimistic concurrency, and dependency
/// cascade.
/// </summary>
[ApiController]
[Route("api/v1/capabilities")]
[Authorize]
public class CapabilitiesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Phase 4 Phase-A — full capability descriptor for the installation.
    /// Phase B's UI calls this on login and on SignalR <c>capabilityChanged</c>.
    /// </summary>
    /// <remarks>
    /// Bootstrap-exempt per 4D §3.5 — the descriptor must be readable even if
    /// every capability is disabled, otherwise the admin UI cannot recover.
    /// </remarks>
    [HttpGet("descriptor")]
    [CapabilityBootstrap]
    public async Task<ActionResult<CapabilityDescriptorResponseModel>> GetDescriptor()
    {
        var result = await mediator.Send(new GetCapabilityDescriptorQuery());
        return Ok(result);
    }

    /// <summary>
    /// Phase 4 Phase-B — toggles a single capability's enabled flag. Refreshes
    /// the in-memory snapshot, writes a system audit row, and broadcasts a
    /// SignalR <c>capabilityChanged</c> event to all connected clients.
    /// </summary>
    /// <remarks>
    /// Bootstrap-exempt per 4D §3.5 — the admin endpoint that mutates
    /// capability state cannot itself be gated by capability state, otherwise
    /// disabling CAP-IDEN-CAPABILITY-ADMIN would lock the install out of
    /// recovery. Authorization is enforced via the Admin role guard.
    ///
    /// Phase B intentionally keeps this minimum viable: no optimistic
    /// concurrency (4D §3.4 / Phase C), no dependency-cascade or mutex peer
    /// checks (4D §8.2-8.3 / Phase C), no preset apply (4D §5.1 / Phase C).
    /// </remarks>
    [HttpPut("{id}/enabled")]
    [Authorize(Roles = "Admin")]
    [CapabilityBootstrap]
    public async Task<ActionResult<CapabilityDescriptorEntry>> SetEnabled(
        string id,
        [FromBody] ToggleCapabilityRequestModel body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new ToggleCapabilityCommand(id, body.Enabled), ct);
        return Ok(result);
    }
}
