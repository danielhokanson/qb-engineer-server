namespace QBEngineer.Core.Models;

public record ChatMessageEvent(
    int Id,
    int SenderId,
    string SenderName,
    string SenderInitials,
    string SenderColor,
    int RecipientId,
    string Content,
    DateTime CreatedAt);
