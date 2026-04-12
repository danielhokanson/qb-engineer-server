using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Bi;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/bi-api-keys")]
[Authorize(Roles = "Admin")]
public class BiApiKeysController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BiApiKeyResponseModel>>> GetApiKeys()
    {
        var result = await mediator.Send(new GetBiApiKeysQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateBiApiKeyResponseModel>> CreateApiKey(
        [FromBody] CreateBiApiKeyRequestModel model)
    {
        var result = await mediator.Send(new CreateBiApiKeyCommand(model));
        return Created($"/api/v1/admin/bi-api-keys/{result.Id}", result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RevokeApiKey(int id)
    {
        await mediator.Send(new RevokeBiApiKeyCommand(id));
        return NoContent();
    }
}
