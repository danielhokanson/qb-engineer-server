using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Admin;
using QBEngineer.Api.Features.ReferenceData;
using QBEngineer.Api.Features.TrackTypes;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IMediator mediator) : ControllerBase
{
    // ── Users ──

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserResponseModel>>> GetUsers()
    {
        var result = await mediator.Send(new GetAdminUsersQuery());
        return Ok(result);
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserResponseModel>> CreateUser(CreateAdminUserCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetUsers), result);
    }

    [HttpPut("users/{id:int}")]
    public async Task<ActionResult<AdminUserResponseModel>> UpdateUser(int id, UpdateAdminUserCommand command)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    // ── Track Types ──

    [HttpGet("track-types")]
    public async Task<ActionResult<List<TrackTypeResponseModel>>> GetTrackTypes()
    {
        var result = await mediator.Send(new GetTrackTypesQuery());
        return Ok(result);
    }

    // ── Reference Data ──

    [HttpGet("reference-data")]
    public async Task<ActionResult<List<ReferenceDataGroupResponseModel>>> GetReferenceData()
    {
        var result = await mediator.Send(new GetReferenceDataGroupsQuery());
        return Ok(result);
    }

    [HttpPost("reference-data")]
    public async Task<ActionResult<ReferenceDataResponseModel>> CreateReferenceData(CreateReferenceDataCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetReferenceData), result);
    }

    [HttpPut("reference-data/{id:int}")]
    public async Task<ActionResult<ReferenceDataResponseModel>> UpdateReferenceData(int id, UpdateReferenceDataCommand command)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }
}
