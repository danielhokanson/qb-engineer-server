using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record ChatRoomMemberResponseModel(
    int UserId,
    string DisplayName,
    string Initials,
    string Color,
    ChannelMemberRole Role = ChannelMemberRole.Member,
    bool IsMuted = false);
