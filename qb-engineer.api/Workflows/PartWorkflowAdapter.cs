using System.Text.Json;

using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Workflows;

/// <summary>
/// Workflow Pattern Phase 3 — Part-specific creator + field-applier wired
/// into the workflow runtime. The applier reads a small known set of
/// scalar fields (description, partType, material, moldToolRef,
/// externalPartNumber, manualCostOverride). BOM / Operations remain edited
/// via their existing dedicated endpoints; the workflow's BOM and routing
/// step components call those endpoints directly (no need to duplicate
/// nested-entity edits through this applier).
/// </summary>
public class PartWorkflowAdapter(AppDbContext db, IPartRepository repo)
    : IWorkflowEntityCreator, IWorkflowFieldApplier, IWorkflowEntityPromoter
{
    public string EntityType => "Part";

    public async Task<int> CreateDraftAsync(JsonElement? initialData, CancellationToken ct)
    {
        var partType = ReadEnumOrDefault(initialData, "partType", PartType.Part);
        // Pillar 1 — read the three orthogonal axes when present. Fall back to
        // legacy partType-derived defaults so older fork-dialog payloads
        // (still on the wire during phased rollout) keep working.
        var procurement = ReadEnumOrDefault(
            initialData, "procurementSource",
            DefaultProcurementForLegacyPartType(partType));
        var inventoryClass = ReadEnumOrDefault(
            initialData, "inventoryClass",
            DefaultInventoryClassForLegacyPartType(partType));
        var traceability = ReadEnumOrDefault(initialData, "traceabilityType", TraceabilityType.None);
        var abc = ReadNullableEnum<AbcClass>(initialData, "abcClass");
        var itemKindId = ReadIntOrDefault(initialData, "itemKindId");
        var manufacturerName = ReadStringOrDefault(initialData, "manufacturerName")?.Trim();
        var manufacturerPartNumber = ReadStringOrDefault(initialData, "manufacturerPartNumber")?.Trim();

        // Pillar 2 — Tier 2: measurement profile + valuation.
        var materialSpecId = ReadIntOrDefault(initialData, "materialSpecId");
        var weightEach = ReadDecimalOrDefault(initialData, "weightEach");
        var weightDisplayUnit = ReadStringOrDefault(initialData, "weightDisplayUnit")?.Trim();
        var lengthMm = ReadDecimalOrDefault(initialData, "lengthMm");
        var widthMm = ReadDecimalOrDefault(initialData, "widthMm");
        var heightMm = ReadDecimalOrDefault(initialData, "heightMm");
        var dimensionDisplayUnit = ReadStringOrDefault(initialData, "dimensionDisplayUnit")?.Trim();
        var volumeMl = ReadDecimalOrDefault(initialData, "volumeMl");
        var volumeDisplayUnit = ReadStringOrDefault(initialData, "volumeDisplayUnit")?.Trim();
        var valuationClassId = ReadIntOrDefault(initialData, "valuationClassId");

        // Pillar 2 — Tier 3: compliance + classification.
        var htsCode = ReadStringOrDefault(initialData, "htsCode")?.Trim();
        var hazmatClass = ReadStringOrDefault(initialData, "hazmatClass")?.Trim();
        var shelfLifeDays = ReadIntOrDefault(initialData, "shelfLifeDays");
        var backflushPolicy = ReadNullableEnum<BackflushPolicy>(initialData, "backflushPolicy");
        var isKit = ReadBoolOrDefault(initialData, "isKit") ?? false;
        var isConfigurable = ReadBoolOrDefault(initialData, "isConfigurable") ?? false;
        var defaultBinId = ReadIntOrDefault(initialData, "defaultBinId");
        var sourcePartId = ReadIntOrDefault(initialData, "sourcePartId");

        // Phase-4 deferred-materialization: the workflow only calls this once
        // the user has submitted the first step's fields, so `name` should
        // always be present. If it isn't, we surface a 400 rather than save a
        // placeholder row — the entity row should not exist until it has a
        // real name.
        var name = ReadStringOrDefault(initialData, "name")
                   ?? ReadStringOrDefault(initialData, "description"); // backward-compat for older fork payloads
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException(
                "Part requires a name to materialize.",
                new[] { new ValidationFailure("name", "Name is required.") });
        }
        var description = ReadStringOrDefault(initialData, "description");

        var partNumber = await repo.GetNextPartNumberAsync(partType, ct);

        var part = new Part
        {
            PartNumber = partNumber,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Revision = ReadStringOrDefault(initialData, "revision")?.Trim() ?? "A",
            PartType = partType,
            ProcurementSource = procurement,
            InventoryClass = inventoryClass,
            ItemKindId = itemKindId,
            TraceabilityType = traceability,
            AbcClass = abc,
            ManufacturerName = manufacturerName,
            ManufacturerPartNumber = manufacturerPartNumber,
            Status = PartStatus.Draft,
            Material = ReadStringOrDefault(initialData, "material")?.Trim(),
            MaterialSpecId = materialSpecId,
            MoldToolRef = ReadStringOrDefault(initialData, "moldToolRef")?.Trim(),
            ExternalPartNumber = ReadStringOrDefault(initialData, "externalPartNumber")?.Trim(),
            ManualCostOverride = ReadDecimalOrDefault(initialData, "manualCostOverride"),
            // Pillar 2 — Tier 2 measurement profile + valuation.
            WeightEach = weightEach,
            WeightDisplayUnit = weightDisplayUnit,
            LengthMm = lengthMm,
            WidthMm = widthMm,
            HeightMm = heightMm,
            DimensionDisplayUnit = dimensionDisplayUnit,
            VolumeMl = volumeMl,
            VolumeDisplayUnit = volumeDisplayUnit,
            ValuationClassId = valuationClassId,
            // Pillar 2 — Tier 3 compliance + classification.
            HtsCode = htsCode,
            HazmatClass = hazmatClass,
            ShelfLifeDays = shelfLifeDays,
            BackflushPolicy = backflushPolicy,
            IsKit = isKit,
            IsConfigurable = isConfigurable,
            DefaultBinId = defaultBinId,
            SourcePartId = sourcePartId,
        };
        db.Parts.Add(part);
        // Defer SaveChanges to the orchestrating handler (single transaction).
        return await SavePartAndReturnIdAsync(part, ct);
    }

    private static ProcurementSource DefaultProcurementForLegacyPartType(PartType pt) => pt switch
    {
        PartType.Assembly => ProcurementSource.Make,
        PartType.RawMaterial => ProcurementSource.Buy,
        PartType.Consumable => ProcurementSource.Buy,
        PartType.Tooling => ProcurementSource.Buy,
        PartType.Fastener => ProcurementSource.Buy,
        PartType.Electronic => ProcurementSource.Buy,
        PartType.Packaging => ProcurementSource.Buy,
        _ => ProcurementSource.Buy, // Part catch-all — ambiguous, conservative default
    };

    private static InventoryClass DefaultInventoryClassForLegacyPartType(PartType pt) => pt switch
    {
        PartType.Assembly => InventoryClass.Subassembly,
        PartType.RawMaterial => InventoryClass.Raw,
        PartType.Consumable => InventoryClass.Consumable,
        PartType.Tooling => InventoryClass.Tool,
        PartType.Fastener => InventoryClass.Component,
        PartType.Electronic => InventoryClass.Component,
        PartType.Packaging => InventoryClass.Consumable,
        _ => InventoryClass.Component,
    };

    private async Task<int> SavePartAndReturnIdAsync(Part part, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        return part.Id;
    }

    public async Task ApplyAsync(int entityId, JsonElement fields, CancellationToken ct)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == entityId, ct)
            ?? throw new KeyNotFoundException($"Part id {entityId} not found.");

        if (TryReadString(fields, "name", out var name) && name is not null)
            part.Name = name.Trim();
        if (TryReadString(fields, "description", out var desc))
        {
            // null / whitespace clears the optional Description; non-empty trims & sets.
            var trimmed = desc?.Trim();
            part.Description = string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }
        if (TryReadString(fields, "revision", out var rev) && rev is not null)
            part.Revision = rev.Trim();
        if (TryReadString(fields, "material", out var mat))
            part.Material = mat?.Trim();
        if (TryReadString(fields, "moldToolRef", out var mold))
            part.MoldToolRef = mold?.Trim();
        if (TryReadString(fields, "externalPartNumber", out var ext))
            part.ExternalPartNumber = ext?.Trim();
        if (TryReadDecimal(fields, "manualCostOverride", out var manual))
            part.ManualCostOverride = manual;
        if (TryReadEnum<PartType>(fields, "partType", out var pt))
            part.PartType = pt;
        // Pillar 1 — three-axis applies. We accept either the new axes or
        // the legacy partType; both routes converge on the same row.
        if (TryReadEnum<ProcurementSource>(fields, "procurementSource", out var ps))
            part.ProcurementSource = ps;
        if (TryReadEnum<InventoryClass>(fields, "inventoryClass", out var ic))
            part.InventoryClass = ic;
        if (TryReadInt(fields, "itemKindId", out var ikId))
            part.ItemKindId = ikId;
        if (TryReadEnum<TraceabilityType>(fields, "traceabilityType", out var tt))
            part.TraceabilityType = tt;
        if (TryReadEnum<AbcClass>(fields, "abcClass", out var abc))
            part.AbcClass = abc;
        if (TryReadString(fields, "manufacturerName", out var mn))
            part.ManufacturerName = mn?.Trim();
        if (TryReadString(fields, "manufacturerPartNumber", out var mpn))
            part.ManufacturerPartNumber = mpn?.Trim();

        // Pillar 2 — Tier 2: measurement profile + valuation.
        if (TryReadInt(fields, "materialSpecId", out var msId))
            part.MaterialSpecId = msId;
        if (TryReadDecimal(fields, "weightEach", out var weightEach))
            part.WeightEach = weightEach;
        if (TryReadString(fields, "weightDisplayUnit", out var wdu))
            part.WeightDisplayUnit = wdu?.Trim();
        if (TryReadDecimal(fields, "lengthMm", out var lengthMm))
            part.LengthMm = lengthMm;
        if (TryReadDecimal(fields, "widthMm", out var widthMm))
            part.WidthMm = widthMm;
        if (TryReadDecimal(fields, "heightMm", out var heightMm))
            part.HeightMm = heightMm;
        if (TryReadString(fields, "dimensionDisplayUnit", out var ddu))
            part.DimensionDisplayUnit = ddu?.Trim();
        if (TryReadDecimal(fields, "volumeMl", out var volumeMl))
            part.VolumeMl = volumeMl;
        if (TryReadString(fields, "volumeDisplayUnit", out var vdu))
            part.VolumeDisplayUnit = vdu?.Trim();
        if (TryReadInt(fields, "valuationClassId", out var vcId))
            part.ValuationClassId = vcId;

        // Pillar 2 — Tier 3: compliance + classification.
        if (TryReadString(fields, "htsCode", out var hts))
            part.HtsCode = hts?.Trim();
        if (TryReadString(fields, "hazmatClass", out var hazmat))
            part.HazmatClass = hazmat?.Trim();
        if (TryReadInt(fields, "shelfLifeDays", out var shelfLife))
            part.ShelfLifeDays = shelfLife;
        if (TryReadNullableEnum<BackflushPolicy>(fields, "backflushPolicy", out var bp))
            part.BackflushPolicy = bp;
        if (TryReadBool(fields, "isKit", out var isKit) && isKit.HasValue)
            part.IsKit = isKit.Value;
        if (TryReadBool(fields, "isConfigurable", out var isCfg) && isCfg.HasValue)
            part.IsConfigurable = isCfg.Value;
        if (TryReadInt(fields, "defaultBinId", out var binId))
            part.DefaultBinId = binId;
        if (TryReadInt(fields, "sourcePartId", out var srcId))
            part.SourcePartId = srcId;

        // ─── Pillar 6 follow-up — step-component fields ───
        // The new per-combo step components (PartSourcingStep,
        // PartInventoryStep, PartQualityStep, PartToolAssetStep, etc. shipped
        // in UI commit b8ef771) emit these fields via patchStep. Reads grouped
        // by cluster for readability.

        // Sourcing cluster (Buy* / Subcontract* combos)
        if (TryReadInt(fields, "preferredVendorId", out var prefVendorId))
            part.PreferredVendorId = prefVendorId;
        if (TryReadInt(fields, "leadTimeDays", out var leadTime))
            part.LeadTimeDays = leadTime;
        if (TryReadDecimalAsInt(fields, "minOrderQty", out var minOrderQty))
            part.MinOrderQty = minOrderQty;
        if (TryReadDecimalAsInt(fields, "packSize", out var packSize))
            part.PackSize = packSize;

        // Inventory cluster (every non-Phantom combo)
        if (TryReadDecimal(fields, "minStockThreshold", out var minStock))
            part.MinStockThreshold = minStock;
        if (TryReadDecimal(fields, "reorderPoint", out var reorderPt))
            part.ReorderPoint = reorderPt;
        if (TryReadDecimal(fields, "reorderQuantity", out var reorderQty))
            part.ReorderQuantity = reorderQty;
        if (TryReadInt(fields, "safetyStockDays", out var safetyDays))
            part.SafetyStockDays = safetyDays;

        // UoM cluster (FK to unit_of_measure)
        if (TryReadInt(fields, "stockUomId", out var stockUomId))
            part.StockUomId = stockUomId;
        if (TryReadInt(fields, "purchaseUomId", out var purchaseUomId))
            part.PurchaseUomId = purchaseUomId;
        if (TryReadInt(fields, "salesUomId", out var salesUomId))
            part.SalesUomId = salesUomId;

        // Quality cluster (receiving inspection — B1-B4, M1-M3, S1, S2)
        if (TryReadBool(fields, "requiresReceivingInspection", out var reqInsp) && reqInsp.HasValue)
            part.RequiresReceivingInspection = reqInsp.Value;
        if (TryReadInt(fields, "receivingInspectionTemplateId", out var inspTemplateId))
            part.ReceivingInspectionTemplateId = inspTemplateId;
        if (TryReadEnum<ReceivingInspectionFrequency>(fields, "inspectionFrequency", out var inspFreq))
            part.InspectionFrequency = inspFreq;
        if (TryReadInt(fields, "inspectionSkipAfterN", out var inspSkip))
            part.InspectionSkipAfterN = inspSkip;

        // Tooling cluster (Make+Tool / Buy+Tool)
        if (TryReadInt(fields, "toolingAssetId", out var toolAssetId))
            part.ToolingAssetId = toolAssetId;

        // MRP cluster (Make + Subassembly/FinishedGood, Phantom combos)
        if (TryReadBool(fields, "isMrpPlanned", out var isMrp) && isMrp.HasValue)
            part.IsMrpPlanned = isMrp.Value;
        if (TryReadNullableEnum<LotSizingRule>(fields, "lotSizingRule", out var lotRule))
            part.LotSizingRule = lotRule;
        if (TryReadDecimal(fields, "fixedOrderQuantity", out var foq))
            part.FixedOrderQuantity = foq;
        if (TryReadDecimal(fields, "minimumOrderQuantity", out var moq))
            part.MinimumOrderQuantity = moq;
        if (TryReadDecimal(fields, "orderMultiple", out var ordMult))
            part.OrderMultiple = ordMult;
        if (TryReadInt(fields, "planningFenceDays", out var planFence))
            part.PlanningFenceDays = planFence;
        if (TryReadInt(fields, "demandFenceDays", out var demFence))
            part.DemandFenceDays = demFence;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> SoftDeleteIfDraftAsync(int entityId, CancellationToken ct)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == entityId, ct);
        if (part is null) return false;
        if (part.Status != PartStatus.Draft) return false;
        part.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PromoteAsync(int entityId, string targetStatus, CancellationToken ct)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == entityId, ct);
        if (part is null) return false;
        if (!Enum.TryParse<PartStatus>(targetStatus, true, out var target))
            throw new InvalidOperationException(
                $"'{targetStatus}' is not a valid PartStatus value.");
        if (part.Status == target) return false; // already there
        part.Status = target;
        await db.SaveChangesAsync(ct);
        return true;
    }

    // ─── JSON helpers ───

    private static string? ReadStringOrDefault(JsonElement? root, string name)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return null;
        if (!root.Value.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }

    private static decimal? ReadDecimalOrDefault(JsonElement? root, string name)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return null;
        if (!root.Value.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var d) ? d : null;
    }

    private static int? ReadIntOrDefault(JsonElement? root, string name)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return null;
        if (!root.Value.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var i) ? i : null;
    }

    private static T? ReadNullableEnum<T>(JsonElement? root, string name) where T : struct, Enum
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return null;
        if (!root.Value.TryGetProperty(name, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.Null) return null;
        if (prop.ValueKind == JsonValueKind.String && Enum.TryParse<T>(prop.GetString(), true, out var parsed))
            return parsed;
        return null;
    }

    private static bool TryReadInt(JsonElement root, string name, out int? value)
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.Null) { value = null; return true; }
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var i)) { value = i; return true; }
        return false;
    }

    /// <summary>
    /// Reads a JSON number into <c>int?</c>, accepting decimal values (e.g.
    /// <c>5</c> or <c>5.0</c>) and truncating to int. Used for fields whose
    /// entity type is <c>int?</c> but whose Angular form control is
    /// <c>FormControl&lt;number | null&gt;</c> — JSON has no int/decimal
    /// distinction, so the wire value can come through as either.
    /// </summary>
    private static bool TryReadDecimalAsInt(JsonElement root, string name, out int? value)
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.Null) { value = null; return true; }
        if (prop.ValueKind == JsonValueKind.Number)
        {
            if (prop.TryGetInt32(out var i)) { value = i; return true; }
            if (prop.TryGetDecimal(out var d)) { value = (int)d; return true; }
        }
        return false;
    }

    private static T ReadEnumOrDefault<T>(JsonElement? root, string name, T defaultValue) where T : struct, Enum
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return defaultValue;
        if (!root.Value.TryGetProperty(name, out var prop)) return defaultValue;
        if (prop.ValueKind == JsonValueKind.String && Enum.TryParse<T>(prop.GetString(), true, out var parsed))
            return parsed;
        return defaultValue;
    }

    private static bool TryReadString(JsonElement root, string name, out string? value)
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.Null) { value = null; return true; }
        if (prop.ValueKind == JsonValueKind.String) { value = prop.GetString(); return true; }
        return false;
    }

    private static bool TryReadDecimal(JsonElement root, string name, out decimal? value)
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.Null) { value = null; return true; }
        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var d)) { value = d; return true; }
        return false;
    }

    private static bool TryReadEnum<T>(JsonElement root, string name, out T value) where T : struct, Enum
    {
        value = default;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.String && Enum.TryParse<T>(prop.GetString(), true, out var parsed))
        {
            value = parsed;
            return true;
        }
        return false;
    }

    private static bool TryReadNullableEnum<T>(JsonElement root, string name, out T? value) where T : struct, Enum
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        if (prop.ValueKind == JsonValueKind.Null) { value = null; return true; }
        if (prop.ValueKind == JsonValueKind.String && Enum.TryParse<T>(prop.GetString(), true, out var parsed))
        {
            value = parsed;
            return true;
        }
        return false;
    }

    private static bool? ReadBoolOrDefault(JsonElement? root, string name)
    {
        if (root is null || root.Value.ValueKind != JsonValueKind.Object) return null;
        if (!root.Value.TryGetProperty(name, out var prop)) return null;
        return prop.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }

    private static bool TryReadBool(JsonElement root, string name, out bool? value)
    {
        value = null;
        if (root.ValueKind != JsonValueKind.Object) return false;
        if (!root.TryGetProperty(name, out var prop)) return false;
        switch (prop.ValueKind)
        {
            case JsonValueKind.Null: value = null; return true;
            case JsonValueKind.True: value = true; return true;
            case JsonValueKind.False: value = false; return true;
            default: return false;
        }
    }
}
