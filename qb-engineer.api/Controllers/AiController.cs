using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using QBEngineer.Api.Features.Ai;
using QBEngineer.Api.Features.AiAssistants;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/ai")]
[Authorize]
public class AiController(IMediator mediator) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var result = await mediator.Send(new CheckAiAvailabilityQuery(), ct);
        return Ok(result);
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateTextCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeTextCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("search-suggest")]
    public async Task<IActionResult> SearchSuggest([FromBody] AiSearchSuggestCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("help")]
    public async Task<IActionResult> HelpChat([FromBody] AiHelpChatCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("search")]
    public async Task<IActionResult> RagSearch([FromBody] RagSearchCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument([FromBody] IndexDocumentRequestModel request, CancellationToken ct)
    {
        var chunkCount = await mediator.Send(new IndexDocumentCommand(request.EntityType, request.EntityId), ct);
        return Ok(new { chunksIndexed = chunkCount });
    }

    [HttpPost("bulk-index")]
    public async Task<IActionResult> BulkIndex([FromBody] BulkIndexDocumentsCommand command, CancellationToken ct)
    {
        var totalChunks = await mediator.Send(command, ct);
        return Ok(new { totalChunksIndexed = totalChunks });
    }

    [HttpPost("assistants/{assistantId:int}/chat")]
    public async Task<IActionResult> AssistantChat(int assistantId, [FromBody] AssistantChatBody body, CancellationToken ct)
    {
        var result = await mediator.Send(new AssistantChatCommand(assistantId, body.Question, body.History), ct);
        return Ok(result);
    }
}

public record AssistantChatBody(string Question, List<AiHelpMessage>? History = null);
