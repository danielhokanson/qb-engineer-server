using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Chat;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/chat")]
[Authorize]
public class ChatController(IMediator mediator) : ControllerBase
{
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ChatConversationResponseModel>>> GetConversations()
    {
        var result = await mediator.Send(new GetConversationsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpGet("messages/{otherUserId:int}")]
    public async Task<ActionResult<List<ChatMessageResponseModel>>> GetMessages(int otherUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await mediator.Send(new GetMessagesQuery(GetUserId(), otherUserId, page, pageSize));
        return Ok(result);
    }

    [HttpPost("messages")]
    public async Task<ActionResult<ChatMessageResponseModel>> SendMessage([FromBody] SendChatMessageRequestModel model)
    {
        var result = await mediator.Send(new SendMessageCommand(GetUserId(), model.RecipientId, model.Content));
        return CreatedAtAction(nameof(GetMessages), new { otherUserId = model.RecipientId }, result);
    }

    [HttpPost("messages/{otherUserId:int}/read")]
    public async Task<IActionResult> MarkRead(int otherUserId)
    {
        await mediator.Send(new MarkMessagesReadCommand(GetUserId(), otherUserId));
        return NoContent();
    }

    // ── Chat Rooms (Group Chat) ──

    [HttpGet("rooms")]
    public async Task<ActionResult<List<ChatRoomResponseModel>>> GetRooms()
    {
        var result = await mediator.Send(new GetChatRoomsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpPost("rooms")]
    public async Task<ActionResult<ChatRoomResponseModel>> CreateRoom([FromBody] CreateChatRoomRequestModel model)
    {
        var result = await mediator.Send(new CreateChatRoomCommand(GetUserId(), model.Name, model.MemberIds));
        return CreatedAtAction(nameof(GetRooms), result);
    }

    [HttpGet("rooms/{roomId:int}/messages")]
    public async Task<ActionResult<List<ChatMessageResponseModel>>> GetRoomMessages(
        int roomId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await mediator.Send(new GetChatRoomMessagesQuery(GetUserId(), roomId, page, pageSize));
        return Ok(result);
    }

    [HttpPost("rooms/{roomId:int}/messages")]
    public async Task<ActionResult<ChatMessageResponseModel>> SendRoomMessage(
        int roomId, [FromBody] SendChatMessageRequestModel model)
    {
        var result = await mediator.Send(new SendChatRoomMessageCommand(
            GetUserId(), roomId, model.Content));
        return CreatedAtAction(nameof(GetRoomMessages), new { roomId }, result);
    }

    [HttpPost("rooms/{roomId:int}/members/{userId:int}")]
    public async Task<IActionResult> AddRoomMember(int roomId, int userId)
    {
        await mediator.Send(new AddChatRoomMemberCommand(roomId, userId, GetUserId()));
        return NoContent();
    }

    [HttpDelete("rooms/{roomId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveRoomMember(int roomId, int userId)
    {
        await mediator.Send(new RemoveChatRoomMemberCommand(roomId, userId, GetUserId()));
        return NoContent();
    }
}
