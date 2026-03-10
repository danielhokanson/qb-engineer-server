namespace QBEngineer.Core.Models;

public record ChatRoomMemberResponseModel(
    int UserId,
    string DisplayName,
    string Initials,
    string Color);
