using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Leads;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/leads")]
[Authorize]
public class LeadsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeadResponseModel>>> GetLeads(
        [FromQuery] LeadStatus? status,
        [FromQuery] string? search)
    {
        var result = await mediator.Send(new GetLeadsQuery(status, search));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeadResponseModel>> GetLeadById(int id)
    {
        var result = await mediator.Send(new GetLeadByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LeadResponseModel>> CreateLead([FromBody] CreateLeadRequestModel request)
    {
        var result = await mediator.Send(new CreateLeadCommand(request));
        return Created($"/api/v1/leads/{result.Id}", result);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<LeadResponseModel>> UpdateLead(int id, [FromBody] UpdateLeadRequestModel request)
    {
        var result = await mediator.Send(new UpdateLeadCommand(id, request));
        return Ok(result);
    }
}
