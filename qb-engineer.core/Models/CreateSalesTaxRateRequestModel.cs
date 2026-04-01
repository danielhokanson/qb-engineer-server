namespace QBEngineer.Core.Models;

public record CreateSalesTaxRateRequestModel(
    string Name,
    string Code,
    string? StateCode,
    decimal Rate,
    /// <summary>UTC DateTimeOffset when this rate takes effect. Defaults to now if not provided.</summary>
    DateTimeOffset? EffectiveFrom,
    bool IsDefault,
    string? Description);
