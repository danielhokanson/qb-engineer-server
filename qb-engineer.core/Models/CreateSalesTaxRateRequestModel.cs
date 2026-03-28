namespace QBEngineer.Core.Models;

public record CreateSalesTaxRateRequestModel(
    string Name,
    string Code,
    string? StateCode,
    decimal Rate,
    /// <summary>UTC datetime when this rate takes effect. Defaults to now if not provided.</summary>
    DateTime? EffectiveFrom,
    bool IsDefault,
    string? Description);
