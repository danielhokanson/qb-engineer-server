namespace QBEngineer.Core.Models;

public record ChatRoomResponseModel(
    int Id,
    string Name,
    bool IsGroup,
    int CreatedById,
    DateTimeOffset CreatedAt,
    List<ChatRoomMemberResponseModel> Members);
