using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Capabilities;
using QBEngineer.Api.Features.Capabilities.Descriptor;

namespace QBEngineer.Api.Controllers;

/// <summary>
/// Phase 4 Phase-A — Read-only capability descriptor surface.
/// Mutations land in Phase C under <c>/api/v1/admin/capabilities/</c>.
/// </summary>
[ApiController]
[Route("api/v1/capabilities")]
[Authorize]
[CapabilityBootstrap] // Per 4D §3.5 the descriptor must be readable even if all admin caps are disabled.
public class CapabilitiesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Phase 4 Phase-A — full capability descriptor for the installation.
    /// Phase B's UI calls this on login and on SignalR <c>capability:changed</c>.
    /// </summary>
    [HttpGet("descriptor")]
    public async Task<ActionResult<CapabilityDescriptorResponseModel>> GetDescriptor()
    {
        var result = await mediator.Send(new GetCapabilityDescriptorQuery());
        return Ok(result);
    }
}
