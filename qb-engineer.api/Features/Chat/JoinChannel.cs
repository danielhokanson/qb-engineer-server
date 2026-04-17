using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Chat;

public record JoinChannelCommand(int UserId, int ChannelId) : IRequest;

public class JoinChannelHandler(AppDbContext db) : IRequestHandler<JoinChannelCommand>
{
    public async Task Handle(JoinChannelCommand request, CancellationToken ct)
    {
        var room = await db.Set<ChatRoom>()
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == request.ChannelId, ct)
            ?? throw new KeyNotFoundException($"Channel {request.ChannelId} not found");

        // Only Custom channels are joinable without invitation
        if (room.ChannelType != ChannelType.Custom)
            throw new InvalidOperationException("This channel cannot be joined directly.");

        if (room.Members.Any(m => m.UserId == request.UserId))
            return; // Already a member

        room.Members.Add(new ChatRoomMember
        {
            UserId = request.UserId,
            JoinedAt = DateTimeOffset.UtcNow,
            Role = ChannelMemberRole.Member,
        });

        await db.SaveChangesAsync(ct);
    }
}
