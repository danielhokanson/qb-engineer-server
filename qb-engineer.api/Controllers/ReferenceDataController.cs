using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.ReferenceData;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/reference-data")]
[Authorize]
public class ReferenceDataController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ReferenceDataGroupDto>>> GetGroups()
    {
        var result = await mediator.Send(new GetReferenceDataGroupsQuery());
        return Ok(result);
    }

    [HttpGet("{groupCode}")]
    public async Task<ActionResult<List<ReferenceDataDto>>> GetByGroup(string groupCode)
    {
        var result = await mediator.Send(new GetReferenceDataByGroupQuery(groupCode));
        return Ok(result);
    }
}
