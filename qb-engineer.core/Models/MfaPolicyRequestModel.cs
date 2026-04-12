namespace QBEngineer.Core.Models;

public record MfaPolicyRequestModel
{
    public IReadOnlyList<string> RequiredRoles { get; init; } = [];
}
