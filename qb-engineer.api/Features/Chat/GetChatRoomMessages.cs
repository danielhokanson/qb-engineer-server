using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Chat;

public record GetChatRoomMessagesQuery(int UserId, int RoomId, int Page, int PageSize) : IRequest<List<ChatMessageResponseModel>>;

public class GetChatRoomMessagesHandler(AppDbContext db) : IRequestHandler<GetChatRoomMessagesQuery, List<ChatMessageResponseModel>>
{
    public async Task<List<ChatMessageResponseModel>> Handle(GetChatRoomMessagesQuery request, CancellationToken ct)
    {
        // Verify user is member of room
        var isMember = await db.Set<Core.Entities.ChatRoomMember>()
            .AnyAsync(m => m.ChatRoomId == request.RoomId && m.UserId == request.UserId, ct);

        if (!isMember)
            throw new UnauthorizedAccessException("You are not a member of this chat room.");

        var messages = await db.ChatMessages
            .Where(m => m.ChatRoomId == request.RoomId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(db.Users, m => m.SenderId, u => u.Id, (m, u) => new ChatMessageResponseModel(
                m.Id,
                m.SenderId,
                (u.FirstName + " " + u.LastName).Trim(),
                u.Initials ?? "??",
                u.AvatarColor ?? "#94a3b8",
                m.RecipientId,
                m.Content,
                m.IsRead,
                m.CreatedAt))
            .ToListAsync(ct);

        messages.Reverse();
        return messages;
    }
}
