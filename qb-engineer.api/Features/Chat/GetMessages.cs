using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Chat;

public record GetMessagesQuery(int UserId, int OtherUserId, int Page = 1, int PageSize = 50) : IRequest<List<ChatMessageResponseModel>>;

public class GetMessagesHandler(AppDbContext db)
    : IRequestHandler<GetMessagesQuery, List<ChatMessageResponseModel>>
{
    public async Task<List<ChatMessageResponseModel>> Handle(GetMessagesQuery request, CancellationToken ct)
    {
        var messages = await db.ChatMessages
            .Where(m => (m.SenderId == request.UserId && m.RecipientId == request.OtherUserId)
                || (m.SenderId == request.OtherUserId && m.RecipientId == request.UserId))
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

        // Mark unread messages as read
        var unreadIds = await db.ChatMessages
            .Where(m => m.SenderId == request.OtherUserId && m.RecipientId == request.UserId && !m.IsRead)
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (unreadIds.Count > 0)
        {
            await db.ChatMessages
                .Where(m => unreadIds.Contains(m.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.IsRead, true)
                    .SetProperty(m => m.ReadAt, DateTime.UtcNow), ct);
        }

        messages.Reverse();
        return messages;
    }
}
