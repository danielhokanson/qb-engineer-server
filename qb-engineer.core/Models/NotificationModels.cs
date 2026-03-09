namespace QBEngineer.Core.Models;

public record NotificationResponseModel(
    int Id,
    string Type,
    string Severity,
    string Source,
    string Title,
    string Message,
    bool IsRead,
    bool IsPinned,
    bool IsDismissed,
    string? EntityType,
    int? EntityId,
    string? SenderInitials,
    string? SenderColor,
    DateTime CreatedAt);

public record CreateNotificationRequestModel(
    int UserId,
    string Type,
    string Severity,
    string Source,
    string Title,
    string Message,
    string? EntityType,
    int? EntityId,
    int? SenderId);

public record UpdateNotificationRequestModel(
    bool? IsRead,
    bool? IsPinned,
    bool? IsDismissed);
