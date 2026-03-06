using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Jobs;
using QBEngineer.Api.Features.Jobs.Subtasks;
using QBEngineer.Core.Enums;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<JobListDto>>> GetJobs(
        [FromQuery] int? trackTypeId,
        [FromQuery] int? stageId,
        [FromQuery] int? assigneeId,
        [FromQuery] bool isArchived = false,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetJobsQuery(trackTypeId, stageId, assigneeId, isArchived, search));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobDetailDto>> GetJob(int id)
    {
        var result = await mediator.Send(new GetJobByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<JobDetailDto>> CreateJob(CreateJobCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JobDetailDto>> UpdateJob(int id, UpdateJobCommand command)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPatch("{id:int}/stage")]
    public async Task<ActionResult<JobDetailDto>> MoveJobStage(int id, MoveJobStageCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPatch("{id:int}/position")]
    public async Task<ActionResult> UpdateJobPosition(int id, UpdateJobPositionCommand command)
    {
        var cmd = command with { JobId = id };
        await mediator.Send(cmd);
        return NoContent();
    }

    [HttpGet("{id:int}/activity")]
    public async Task<ActionResult<List<ActivityDto>>> GetJobActivity(int id)
    {
        var result = await mediator.Send(new GetJobActivityQuery(id));
        return Ok(result);
    }

    [HttpGet("{id:int}/subtasks")]
    public async Task<ActionResult<List<SubtaskDto>>> GetSubtasks(int id)
    {
        var result = await mediator.Send(new GetSubtasksQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/subtasks")]
    public async Task<ActionResult<SubtaskDto>> CreateSubtask(int id, CreateSubtaskCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetSubtasks), new { id }, result);
    }

    [HttpPatch("{id:int}/subtasks/{subtaskId:int}")]
    public async Task<ActionResult<SubtaskDto>> UpdateSubtask(int id, int subtaskId, UpdateSubtaskCommand command)
    {
        var cmd = command with { JobId = id, SubtaskId = subtaskId };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }
}
