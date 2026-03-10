namespace QBEngineer.Core.Models;

public record ChatMessageResponseModel(
    int Id,
    int SenderId,
    string SenderName,
    string SenderInitials,
    string SenderColor,
    int RecipientId,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
