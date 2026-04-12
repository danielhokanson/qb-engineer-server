using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Jobs;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Roles = "Admin,Manager,Engineer,PM")]
public class SubcontractController(IMediator mediator) : ControllerBase
{
    [HttpPost("jobs/{jobId:int}/operations/{opId:int}/send-out")]
    public async Task<ActionResult<SubcontractOrderResponseModel>> SendOut(int jobId, int opId, [FromBody] SendOutRequestModel data)
    {
        var result = await mediator.Send(new SendOutSubcontractCommand(jobId, opId, data));
        return CreatedAtAction(nameof(GetSubcontractOrders), new { jobId }, result);
    }

    [HttpPost("subcontract-orders/{id:int}/receive")]
    public async Task<ActionResult<SubcontractOrderResponseModel>> ReceiveBack(int id, [FromBody] ReceiveBackRequestModel data)
    {
        var result = await mediator.Send(new ReceiveBackSubcontractCommand(id, data));
        return Ok(result);
    }

    [HttpGet("jobs/{jobId:int}/subcontract-orders")]
    public async Task<ActionResult<List<SubcontractOrderResponseModel>>> GetSubcontractOrders(int jobId)
    {
        var result = await mediator.Send(new GetSubcontractOrdersQuery(jobId));
        return Ok(result);
    }
}
