using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record PartDetailResponseModel(
    int Id,
    string PartNumber,
    string Description,
    string Revision,
    PartStatus Status,
    PartType PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    int? PreferredVendorId,
    string? PreferredVendorName,
    decimal? MinStockThreshold,
    decimal? ReorderPoint,
    List<BOMEntryResponseModel> BomEntries,
    List<BOMUsageResponseModel> UsedIn,
    DateTime CreatedAt,
    DateTime UpdatedAt);
