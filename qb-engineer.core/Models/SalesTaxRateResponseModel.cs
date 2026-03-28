namespace QBEngineer.Core.Models;

public record SalesTaxRateResponseModel(
    int Id,
    string Name,
    string Code,
    string? StateCode,
    decimal Rate,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    bool IsDefault,
    bool IsActive,
    string? Description);
