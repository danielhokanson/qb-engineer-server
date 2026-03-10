namespace QBEngineer.Core.Models;

public record SalesTaxRateResponseModel(
    int Id,
    string Name,
    string Code,
    decimal Rate,
    bool IsDefault,
    bool IsActive,
    string? Description);
