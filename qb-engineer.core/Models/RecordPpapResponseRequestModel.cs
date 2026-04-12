using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record RecordPpapResponseRequestModel
{
    public PpapStatus CustomerDecision { get; init; }
    public string? Notes { get; init; }
}
