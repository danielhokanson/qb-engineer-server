using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.TrackTypes;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/track-types")]
[Authorize]
public class TrackTypesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TrackTypeResponseModel>>> GetTrackTypes()
    {
        var result = await mediator.Send(new GetTrackTypesQuery());
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TrackTypeResponseModel>> GetTrackType(int id)
    {
        var result = await mediator.Send(new GetTrackTypeByIdQuery(id));
        return Ok(result);
    }
}
