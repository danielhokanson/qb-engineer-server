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
    string? ExternalPartNumber,
    string? ExternalId,
    string? ExternalRef,
    string? Provider,
    int? PreferredVendorId,
    string? PreferredVendorName,
    decimal? MinStockThreshold,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    int? SafetyStockDays,
    bool IsSerialTracked,
    int? ToolingAssetId,
    string? ToolingAssetName,
    List<BOMEntryResponseModel> BomEntries,
    List<BOMUsageResponseModel> UsedIn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
