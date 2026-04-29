using System.Text.Json;

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
        var description = ReadStringOrDefault(initialData, "description") ?? "(Draft)";

        var partNumber = await repo.GetNextPartNumberAsync(partType, ct);

        var part = new Part
        {
            PartNumber = partNumber,
            Description = description.Trim(),
            Revision = ReadStringOrDefault(initialData, "revision")?.Trim() ?? "A",
            PartType = partType,
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

    private async Task<int> SavePartAndReturnIdAsync(Part part, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
        return part.Id;
    }

    public async Task ApplyAsync(int entityId, JsonElement fields, CancellationToken ct)
    {
        var part = await db.Parts.FirstOrDefaultAsync(p => p.Id == entityId, ct)
            ?? throw new KeyNotFoundException($"Part id {entityId} not found.");

        if (TryReadString(fields, "description", out var desc) && desc is not null)
            part.Description = desc.Trim();
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
