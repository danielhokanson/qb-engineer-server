using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.IoT;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/shop-floor/machine")]
[Authorize]
public class ShopFloorMachineController(IMediator mediator) : ControllerBase
{
    [HttpGet("{workCenterId:int}/live")]
    public async Task<ActionResult<List<MachineDataPointResponseModel>>> GetLatestValues(int workCenterId)
    {
        var result = await mediator.Send(new GetMachineTagLatestQuery(workCenterId));
        return Ok(result);
    }

    [HttpGet("{workCenterId:int}/history")]
    public async Task<ActionResult<List<MachineDataPointResponseModel>>> GetHistory(
        int workCenterId,
        [FromQuery] int? tagId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to)
    {
        var result = await mediator.Send(new GetMachineTagHistoryQuery(workCenterId, tagId, from, to));
        return Ok(result);
    }
}
