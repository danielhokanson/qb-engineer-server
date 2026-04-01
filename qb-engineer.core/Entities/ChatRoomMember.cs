namespace QBEngineer.Core.Entities;

public class ChatRoomMember : BaseAuditableEntity
{
    public int ChatRoomId { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset JoinedAt { get; set; }

    public ChatRoom ChatRoom { get; set; } = null!;
}
