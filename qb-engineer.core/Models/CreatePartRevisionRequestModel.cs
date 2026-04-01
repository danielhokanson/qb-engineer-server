namespace QBEngineer.Core.Models;

public record CreatePartRevisionRequestModel(
    string Revision,
    string? ChangeDescription,
    string? ChangeReason,
    DateTimeOffset EffectiveDate);
