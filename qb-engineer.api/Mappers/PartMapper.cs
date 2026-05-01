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
            // Pillar 1 — Decomposed type axes
            ProcurementSource: part.ProcurementSource,
            InventoryClass: part.InventoryClass,
            ItemKindId: part.ItemKindId,
            ItemKindLabel: part.ItemKind?.Label,
            // Tier 0 additions
            TraceabilityType: part.TraceabilityType,
            AbcClass: part.AbcClass,
            ManufacturerName: part.ManufacturerName,
            ManufacturerPartNumber: part.ManufacturerPartNumber,
            Material: part.Material,
            // Pillar 2 — Tier 2 material spec FK
            MaterialSpecId: part.MaterialSpecId,
            MaterialSpecLabel: part.MaterialSpec?.Label,
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
            // Pillar 2 — Tier 2 measurement profile
            WeightEach: part.WeightEach,
            WeightDisplayUnit: part.WeightDisplayUnit,
            LengthMm: part.LengthMm,
            WidthMm: part.WidthMm,
            HeightMm: part.HeightMm,
            DimensionDisplayUnit: part.DimensionDisplayUnit,
            VolumeMl: part.VolumeMl,
            VolumeDisplayUnit: part.VolumeDisplayUnit,
            // Pillar 2 — Tier 2 valuation
            ValuationClassId: part.ValuationClassId,
            ValuationClassLabel: part.ValuationClass?.Label,
            // Pillar 2 — Tier 3 compliance + classification
            HtsCode: part.HtsCode,
            HazmatClass: part.HazmatClass,
            ShelfLifeDays: part.ShelfLifeDays,
            BackflushPolicy: part.BackflushPolicy,
            IsKit: part.IsKit,
            IsConfigurable: part.IsConfigurable,
            DefaultBinId: part.DefaultBinId,
            SourcePartId: part.SourcePartId,
            BomEntries: bomEntries ?? [],
            UsedIn: usedIn ?? [],
            CreatedAt: part.CreatedAt,
            UpdatedAt: part.UpdatedAt);
    }
}
