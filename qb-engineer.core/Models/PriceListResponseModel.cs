namespace QBEngineer.Core.Models;

public record PriceListResponseModel(
    int Id,
    string Name,
    string? Description,
    int? CustomerId,
    string? CustomerName,
    bool IsDefault,
    bool IsActive,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    List<PriceListEntryResponseModel> Entries,
    DateTime CreatedAt,
    DateTime UpdatedAt);
