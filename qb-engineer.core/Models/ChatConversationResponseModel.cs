namespace QBEngineer.Core.Models;

public record ChatConversationResponseModel(
    int UserId,
    string UserName,
    string UserInitials,
    string UserColor,
    string? LastMessage,
    DateTime? LastMessageAt,
    int UnreadCount);
