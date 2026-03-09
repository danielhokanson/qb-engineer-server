namespace QBEngineer.Core.Models;

public record UserResponseModel(int Id, string Initials, string Name, string Color);

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

public record CreateUserRequestModel(
    string Email,
    string FirstName,
    string LastName,
    string? Initials,
    string? AvatarColor,
    string Password,
    string Role);

public record UpdateUserRequestModel(
    string? FirstName,
    string? LastName,
    string? Initials,
    string? AvatarColor,
    bool? IsActive,
    string? Role);
