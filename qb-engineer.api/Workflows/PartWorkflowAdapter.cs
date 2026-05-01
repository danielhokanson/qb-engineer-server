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
            MoldToolRef = ReadStringOrDefault(initialData, "moldToolRef")?.Trim(),
            ExternalPartNumber = ReadStringOrDefault(initialData, "externalPartNumber")?.Trim(),
            ManualCostOverride = ReadDecimalOrDefault(initialData, "manualCostOverride"),
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
}
