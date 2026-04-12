using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.IoT;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/machine-connections")]
[Authorize(Roles = "Admin,Manager")]
public class MachineConnectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MachineConnectionResponseModel>>> GetConnections(
        [FromQuery] bool? isActive)
    {
        var result = await mediator.Send(new GetMachineConnectionsQuery(isActive));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MachineConnectionResponseModel>> CreateConnection(
        [FromBody] CreateMachineConnectionRequestModel model)
    {
        var result = await mediator.Send(new CreateMachineConnectionCommand(model));
        return CreatedAtAction(nameof(GetConnections), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MachineConnectionResponseModel>> UpdateConnection(
        int id, [FromBody] UpdateMachineConnectionRequestModel model)
    {
        var result = await mediator.Send(new UpdateMachineConnectionCommand(id, model));
        return Ok(result);
    }

    [HttpPost("{id:int}/test")]
    public async Task<ActionResult<TestMachineConnectionResult>> TestConnection(int id)
    {
        var result = await mediator.Send(new TestMachineConnectionCommand(id));
        return Ok(result);
    }
}
