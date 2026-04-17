using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record CreateChannelRequestModel(
    string Name,
    ChannelType ChannelType,
    string? Description,
    string? IconName,
    List<int> MemberIds);
