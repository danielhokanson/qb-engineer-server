using Riok.Mapperly.Abstractions;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Mappers;

[Mapper]
public static partial class PartMapper
{
    /// <summary>
    /// Maps a Part entity to a PartDetailResponseModel.
    /// BOM entries and usage lists must be provided separately as they require sub-mapping.
    /// </summary>
    public static PartDetailResponseModel ToDetailModel(
        this Part part,
        List<BOMEntryResponseModel>? bomEntries = null,
        List<BOMUsageResponseModel>? usedIn = null)
    {
        return new PartDetailResponseModel(
            Id: part.Id,
            PartNumber: part.PartNumber,
            Name: part.Name,
            Description: part.Description,
            Revision: part.Revision,
            Status: part.Status,
            PartType: part.PartType,
            Material: part.Material,
            MoldToolRef: part.MoldToolRef,
            ExternalPartNumber: part.ExternalPartNumber,
            ExternalId: part.ExternalId,
            ExternalRef: part.ExternalRef,
            Provider: part.Provider,
            PreferredVendorId: part.PreferredVendorId,
            PreferredVendorName: part.PreferredVendor?.CompanyName,
            MinStockThreshold: part.MinStockThreshold,
            ReorderPoint: part.ReorderPoint,
            ReorderQuantity: part.ReorderQuantity,
            LeadTimeDays: part.LeadTimeDays,
            SafetyStockDays: part.SafetyStockDays,
            IsSerialTracked: part.IsSerialTracked,
            ToolingAssetId: part.ToolingAssetId,
            ToolingAssetName: part.ToolingAsset?.Name,
            // Workflow Pattern Phase 5 — surfaces cost gates for the hasCost predicate.
            ManualCostOverride: part.ManualCostOverride,
            CurrentCostCalculationId: part.CurrentCostCalculationId,
            BomEntries: bomEntries ?? [],
            UsedIn: usedIn ?? [],
            CreatedAt: part.CreatedAt,
            UpdatedAt: part.UpdatedAt);
    }
}
