using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Files;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class FilesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{entityType}/{entityId:int}/files")]
    public async Task<ActionResult<List<FileAttachmentResponseModel>>> GetFiles(string entityType, int entityId)
    {
        var result = await mediator.Send(new GetFilesQuery(entityType, entityId));
        return Ok(result);
    }

    [HttpPost("{entityType}/{entityId:int}/files")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<FileAttachmentResponseModel>> UploadFile(
        string entityType, int entityId, IFormFile file)
    {
        var result = await mediator.Send(new UploadFileCommand(entityType, entityId, file));
        return Created($"/api/v1/files/{result.Id}", result);
    }

    [HttpGet("files/{id:int}/download")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var result = await mediator.Send(new DownloadFileQuery(id));
        return File(result.Stream, result.ContentType, result.FileName);
    }

    [HttpDelete("files/{id:int}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        await mediator.Send(new DeleteFileCommand(id));
        return NoContent();
    }
}
