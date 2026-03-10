using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Jobs;
using QBEngineer.Api.Features.Jobs.Bulk;
using QBEngineer.Api.Features.Jobs.Links;
using QBEngineer.Api.Features.Jobs.Parts;
using QBEngineer.Api.Features.Jobs.Subtasks;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/jobs")]
[Authorize]
public class JobsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<JobListResponseModel>>> GetJobs(
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
    public async Task<ActionResult<JobDetailResponseModel>> GetJob(int id)
    {
        var result = await mediator.Send(new GetJobByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<JobDetailResponseModel>> CreateJob(CreateJobCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JobDetailResponseModel>> UpdateJob(int id, UpdateJobCommand command)
    {
        var cmd = command with { Id = id };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    [HttpPatch("{id:int}/stage")]
    public async Task<ActionResult<JobDetailResponseModel>> MoveJobStage(int id, MoveJobStageCommand command)
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
    public async Task<ActionResult<List<ActivityResponseModel>>> GetJobActivity(int id)
    {
        var result = await mediator.Send(new GetJobActivityQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<ActivityResponseModel>> CreateJobComment(int id, CreateJobCommentCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return Created($"/api/v1/jobs/{id}/activity", result);
    }

    [HttpGet("{id:int}/subtasks")]
    public async Task<ActionResult<List<SubtaskResponseModel>>> GetSubtasks(int id)
    {
        var result = await mediator.Send(new GetSubtasksQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/subtasks")]
    public async Task<ActionResult<SubtaskResponseModel>> CreateSubtask(int id, CreateSubtaskCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetSubtasks), new { id }, result);
    }

    [HttpPatch("{id:int}/subtasks/{subtaskId:int}")]
    public async Task<ActionResult<SubtaskResponseModel>> UpdateSubtask(int id, int subtaskId, UpdateSubtaskCommand command)
    {
        var cmd = command with { JobId = id, SubtaskId = subtaskId };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    [HttpGet("{id:int}/links")]
    public async Task<ActionResult<List<JobLinkResponseModel>>> GetJobLinks(int id)
    {
        var result = await mediator.Send(new GetJobLinksQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/links")]
    public async Task<ActionResult<JobLinkResponseModel>> CreateJobLink(int id, CreateJobLinkCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetJobLinks), new { id }, result);
    }

    [HttpDelete("{id:int}/links/{linkId:int}")]
    public async Task<ActionResult> DeleteJobLink(int id, int linkId)
    {
        await mediator.Send(new DeleteJobLinkCommand(id, linkId));
        return NoContent();
    }

    // Parts
    [HttpGet("{id:int}/parts")]
    public async Task<ActionResult<List<JobPartResponseModel>>> GetJobParts(int id)
    {
        var result = await mediator.Send(new GetJobPartsQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:int}/parts")]
    public async Task<ActionResult<JobPartResponseModel>> AddJobPart(int id, AddJobPartCommand command)
    {
        var cmd = command with { JobId = id };
        var result = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetJobParts), new { id }, result);
    }

    [HttpPatch("{id:int}/parts/{jobPartId:int}")]
    public async Task<ActionResult<JobPartResponseModel>> UpdateJobPart(int id, int jobPartId, UpdateJobPartCommand command)
    {
        var cmd = command with { JobId = id, JobPartId = jobPartId };
        var result = await mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id:int}/parts/{jobPartId:int}")]
    public async Task<ActionResult> RemoveJobPart(int id, int jobPartId)
    {
        await mediator.Send(new RemoveJobPartCommand(id, jobPartId));
        return NoContent();
    }

    // Bulk operations
    [HttpPatch("bulk/stage")]
    public async Task<ActionResult<BulkOperationResponseModel>> BulkMoveStage(BulkMoveJobStageCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("bulk/assign")]
    public async Task<ActionResult<BulkOperationResponseModel>> BulkAssign(BulkAssignJobCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("bulk/priority")]
    public async Task<ActionResult<BulkOperationResponseModel>> BulkSetPriority(BulkSetPriorityCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("bulk/archive")]
    public async Task<ActionResult<BulkOperationResponseModel>> BulkArchive(BulkArchiveJobsCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}
