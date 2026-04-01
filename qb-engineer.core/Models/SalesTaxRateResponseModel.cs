namespace QBEngineer.Core.Models;

public record SalesTaxRateResponseModel(
    int Id,
    string Name,
    string Code,
    string? StateCode,
    decimal Rate,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    bool IsDefault,
    bool IsActive,
    string? Description);
