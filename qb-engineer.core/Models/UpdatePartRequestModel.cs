using QBEngineer.Core.Enums;

namespace QBEngineer.Core.Models;

public record UpdatePartRequestModel(
    string? Name,
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
    int? SafetyStockDays,
    // Workflow Pattern Phase 5 — manual cost override (Tier 1 single-rate).
    // Sentinel value -1 means "clear to null".
    decimal? ManualCostOverride = null);
