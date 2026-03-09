namespace QBEngineer.Core.Models;

public record AdminUserResponseModel(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    bool IsActive,
    string[] Roles,
    DateTime CreatedAt);
