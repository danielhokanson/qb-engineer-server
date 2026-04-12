using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Admin;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin/plants")]
[Authorize(Roles = "Admin")]
public class PlantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PlantResponseModel>>> GetPlants()
    {
        var result = await mediator.Send(new GetPlantsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PlantResponseModel>> CreatePlant([FromBody] CreatePlantRequestModel request)
    {
        var result = await mediator.Send(new CreatePlantCommand(request));
        return CreatedAtAction(nameof(GetPlants), new { }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePlant(int id, [FromBody] UpdatePlantRequestModel request)
    {
        await mediator.Send(new UpdatePlantCommand(id, request));
        return NoContent();
    }
}
