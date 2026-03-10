namespace QBEngineer.Core.Entities;

public class ChatMessage : BaseAuditableEntity
{
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    // Group chat support
    public int? ChatRoomId { get; set; }
    public ChatRoom? ChatRoom { get; set; }

    // File/image sharing
    public int? FileAttachmentId { get; set; }
    public FileAttachment? FileAttachment { get; set; }

    // Entity link sharing
    public string? LinkedEntityType { get; set; }
    public int? LinkedEntityId { get; set; }
}
