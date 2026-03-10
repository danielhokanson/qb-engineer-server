namespace QBEngineer.Core.Entities;

public class ChatRoom : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public int CreatedById { get; set; }

    public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
