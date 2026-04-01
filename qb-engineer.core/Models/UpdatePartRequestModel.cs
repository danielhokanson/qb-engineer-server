using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdatePartRequestModel(
    string? Description,
    string? Revision,
    PartStatus? Status,
    PartType? PartType,
    string? Material,
    string? MoldToolRef,
    string? ExternalPartNumber,
    int? ToolingAssetId,
    int? PreferredVendorId,
    decimal? MinStockThreshold,
    decimal? ReorderPoint,
    decimal? ReorderQuantity,
    int? LeadTimeDays,
    int? SafetyStockDays);
