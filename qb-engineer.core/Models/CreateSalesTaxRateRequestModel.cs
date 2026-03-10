namespace QBEngineer.Core.Models;

public record CreateSalesTaxRateRequestModel(
    string Name,
    string Code,
    decimal Rate,
    bool IsDefault,
    string? Description);
