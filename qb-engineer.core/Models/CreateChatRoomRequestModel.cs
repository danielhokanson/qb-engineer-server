namespace QBEngineer.Core.Models;

public record CreateChatRoomRequestModel(
    string Name,
    List<int> MemberIds);
