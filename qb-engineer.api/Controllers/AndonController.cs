using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Andon;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/shop-floor/andon")]
[Authorize]
public class AndonController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AndonBoardWorkCenterResponseModel>>> GetBoardData()
    {
        var result = await mediator.Send(new GetAndonBoardDataQuery());
        return Ok(result);
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<List<AndonAlertResponseModel>>> GetAlerts(
        [FromQuery] int? workCenterId,
        [FromQuery] AndonAlertStatus? status)
    {
        var result = await mediator.Send(new GetAndonAlertsQuery(workCenterId, status));
        return Ok(result);
    }

    [HttpPost("alerts")]
    public async Task<ActionResult<AndonAlertResponseModel>> CreateAlert(
        [FromBody] CreateAndonAlertRequestModel model)
    {
        var result = await mediator.Send(new CreateAndonAlertCommand(model));
        return CreatedAtAction(nameof(GetAlerts), new { id = result.Id }, result);
    }

    [HttpPost("alerts/{id:int}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(int id)
    {
        await mediator.Send(new AcknowledgeAndonAlertCommand(id));
        return NoContent();
    }

    [HttpPost("alerts/{id:int}/resolve")]
    public async Task<IActionResult> ResolveAlert(int id, [FromBody] ResolveAndonAlertRequestModel? model)
    {
        await mediator.Send(new ResolveAndonAlertCommand(id, model?.Notes));
        return NoContent();
    }
}
