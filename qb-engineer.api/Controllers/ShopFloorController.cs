using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Search;
using QBEngineer.Api.Features.ShopFloor;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/display/shop-floor")]
[Authorize]
public class ShopFloorController(IMediator mediator) : ControllerBase
{
    private static readonly HashSet<string> KioskEntityTypes = new(["Job", "Part"]);

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ShopFloorOverviewResponseModel>> GetOverview([FromQuery] int? teamId = null)
    {
        var result = await mediator.Send(new GetShopFloorOverviewQuery(teamId));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("clock-status")]
    public async Task<ActionResult<List<ClockWorkerModel>>> GetClockStatus([FromQuery] int? teamId = null)
    {
        var result = await mediator.Send(new GetClockStatusQuery(teamId));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<List<SearchResultModel>>> KioskSearch(
        [FromQuery] string q, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<SearchResultModel>());

        var all = await mediator.Send(new GlobalSearchQuery(q.Trim(), Math.Min(limit, 20)));
        var filtered = all.Where(r => KioskEntityTypes.Contains(r.EntityType)).ToList();
        return Ok(filtered);
    }

    [AllowAnonymous]
    [HttpPost("identify-scan")]
    public async Task<ActionResult<ScanIdentificationResult>> IdentifyScan([FromBody] IdentifyScanRequestModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ScanValue))
            return BadRequest("scanValue is required");

        var result = await mediator.Send(new IdentifyScanQuery(model.ScanValue));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("clock")]
    public async Task<IActionResult> ClockInOut([FromBody] ClockInOutRequestModel model)
    {
        await mediator.Send(new ClockInOutCommand(model.UserId, model.EventType));
        return NoContent();
    }

    // ─── Teams ───
    [AllowAnonymous]
    [HttpGet("teams")]
    public async Task<ActionResult<List<TeamModel>>> GetTeams()
    {
        var result = await mediator.Send(new GetTeamsQuery());
        return Ok(result);
    }

    [HttpPost("teams")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TeamModel>> CreateTeam([FromBody] CreateTeamCommand command)
    {
        var result = await mediator.Send(command);
        return Created($"/api/v1/display/shop-floor/teams/{result.Id}", result);
    }

    [HttpPut("teams/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TeamModel>> UpdateTeam(int id, [FromBody] UpdateTeamCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("teams/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        await mediator.Send(new DeleteTeamCommand(id));
        return NoContent();
    }

    [HttpGet("teams/{id:int}/members")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<TeamMemberModel>>> GetTeamMembers(int id)
    {
        var result = await mediator.Send(new GetTeamMembersQuery(id));
        return Ok(result);
    }

    [HttpPut("teams/{id:int}/members")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AssignTeamMembers(int id, [FromBody] AssignTeamMembersRequestModel model)
    {
        await mediator.Send(new AssignTeamMembersCommand(id, model.UserIds));
        return NoContent();
    }

    // ─── Terminals (Admin) ───
    [HttpGet("terminals")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<List<KioskTerminalModel>>> GetTerminals()
    {
        var result = await mediator.Send(new GetKioskTerminalsQuery());
        return Ok(result);
    }

    // ─── Terminal Setup ───
    [AllowAnonymous]
    [HttpGet("terminal")]
    public async Task<ActionResult<KioskTerminalModel>> GetTerminal([FromQuery] string deviceToken)
    {
        if (string.IsNullOrWhiteSpace(deviceToken))
            return BadRequest("deviceToken is required");

        var result = await mediator.Send(new GetKioskTerminalQuery(deviceToken));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("terminal")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<KioskTerminalModel>> SetupTerminal([FromBody] SetupTerminalRequestModel model)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await mediator.Send(new SetupKioskTerminalCommand(model.Name, model.DeviceToken, model.TeamId, userId));
        return Ok(result);
    }
}

public record SetupTerminalRequestModel(string Name, string DeviceToken, int TeamId);
public record IdentifyScanRequestModel(string ScanValue);
public record AssignTeamMembersRequestModel(List<int> UserIds);
