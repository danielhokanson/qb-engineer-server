namespace QBEngineer.Core.Models;

public record ChatRoomResponseModel(
    int Id,
    string Name,
    bool IsGroup,
    int CreatedById,
    DateTime CreatedAt,
    List<ChatRoomMemberResponseModel> Members);
