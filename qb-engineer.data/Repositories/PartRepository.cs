using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class PartRepository(AppDbContext db) : IPartRepository
{
    public async Task<List<PartListResponseModel>> GetPartsAsync(PartStatus? status, PartType? type, string? search, CancellationToken ct)
    {
        // Legacy non-paged path. Routes through GetPagedAsync with a wide page
        // so existing internal callers behave unchanged. New work should call
        // GetPagedAsync directly. (Phase 3 F7-partial / WU-17.)
        var paged = await GetPagedAsync(new PartListQuery
        {
            Status = status,
            Type = type,
            Q = search,
            PageSize = 200,
            // Legacy callers expected partNumber-asc ordering — preserve that.
            Sort = "partNumber",
            Order = "asc",
        }, ct);
        return paged.Items.ToList();
    }

    public async Task<PagedResponse<PartListResponseModel>> GetPagedAsync(
        PartListQuery query, CancellationToken ct)
    {
        // Phase 3 F7-partial / WU-17 — standardised paged-list contract.
        // Sort whitelisted; stable secondary sort by Id for deterministic
        // pagination across ties.
        var q = db.Parts.AsQueryable();

        // — Filters —
        if (query.Status.HasValue)
            q = q.Where(p => p.Status == query.Status.Value);
        else if (query.IsActive.HasValue)
        {
            // Phase 3 F7-partial: alias IsActive onto the Status enum so the
            // standardised filter dimension works on parts. true = anything
            // not Obsolete; false = Obsolete only.
            q = query.IsActive.Value
                ? q.Where(p => p.Status != PartStatus.Obsolete)
                : q.Where(p => p.Status == PartStatus.Obsolete);
        }

        if (query.Type.HasValue)
            q = q.Where(p => p.PartType == query.Type.Value);

        if (query.DefaultVendorId.HasValue)
            q = q.Where(p => p.PreferredVendorId == query.DefaultVendorId.Value);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLower();
            q = q.Where(p =>
                p.PartNumber.ToLower().Contains(term) ||
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                (p.Material != null && p.Material.ToLower().Contains(term)) ||
                (p.ExternalPartNumber != null && p.ExternalPartNumber.ToLower().Contains(term)));
        }

        if (query.DateFrom.HasValue)
            q = q.Where(p => p.CreatedAt >= query.DateFrom.Value);
        if (query.DateTo.HasValue)
            q = q.Where(p => p.CreatedAt <= query.DateTo.Value);

        // — Count BEFORE paging —
        var totalCount = await q.CountAsync(ct);

        // — Sort (whitelist; default = createdAt desc) —
        var sortKey = (query.Sort ?? "").Trim().ToLowerInvariant();
        var desc = query.OrderDescending;
        IOrderedQueryable<Part> ordered = sortKey switch
        {
            "partnumber"         => desc ? q.OrderByDescending(p => p.PartNumber)         : q.OrderBy(p => p.PartNumber),
            "name"               => desc ? q.OrderByDescending(p => p.Name)               : q.OrderBy(p => p.Name),
            "description"        => desc ? q.OrderByDescending(p => p.Description)        : q.OrderBy(p => p.Description),
            "revision"           => desc ? q.OrderByDescending(p => p.Revision)           : q.OrderBy(p => p.Revision),
            "status"             => desc ? q.OrderByDescending(p => p.Status)             : q.OrderBy(p => p.Status),
            "parttype"           => desc ? q.OrderByDescending(p => p.PartType)           : q.OrderBy(p => p.PartType),
            "type"               => desc ? q.OrderByDescending(p => p.PartType)           : q.OrderBy(p => p.PartType),
            "material"           => desc ? q.OrderByDescending(p => p.Material)           : q.OrderBy(p => p.Material),
            "externalpartnumber" => desc ? q.OrderByDescending(p => p.ExternalPartNumber) : q.OrderBy(p => p.ExternalPartNumber),
            "createdat"          => desc ? q.OrderByDescending(p => p.CreatedAt)          : q.OrderBy(p => p.CreatedAt),
            "updatedat"          => desc ? q.OrderByDescending(p => p.UpdatedAt)          : q.OrderBy(p => p.UpdatedAt),
            "id"                 => desc ? q.OrderByDescending(p => p.Id)                 : q.OrderBy(p => p.Id),
            _ => q.OrderByDescending(p => p.CreatedAt),
        };
        ordered = ordered.ThenBy(p => p.Id);

        var now = DateTimeOffset.UtcNow;

        var items = await ordered
            .Skip(query.Skip)
            .Take(query.EffectivePageSize)
            .Select(p => new PartListResponseModel(
                p.Id,
                p.PartNumber,
                p.Name,
                p.Description,
                p.Revision,
                p.Status,
                p.PartType,
                p.Material,
                p.ExternalPartNumber,
                p.BOMEntries.Count,
                p.CreatedAt,
                db.PartPrices
                    .Where(pp => pp.PartId == p.Id && pp.EffectiveTo == null && pp.EffectiveFrom <= now)
                    .Select(pp => (decimal?)pp.UnitPrice)
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return new PagedResponse<PartListResponseModel>(
            items,
            totalCount,
            query.EffectivePage,
            query.EffectivePageSize);
    }

    public async Task<PartDetailResponseModel?> GetDetailAsync(int id, CancellationToken ct)
    {
        var part = await db.Parts
            .Include(p => p.BOMEntries).ThenInclude(b => b.ChildPart)
            .Include(p => p.UsedInBOM).ThenInclude(b => b.ParentPart)
            .Include(p => p.PreferredVendor)
            .Include(p => p.ToolingAsset)
            .Include(p => p.ItemKind)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (part is null)
            return null;

        var bomEntries = part.BOMEntries
            .OrderBy(b => b.SortOrder)
            .Select(b => new BOMEntryResponseModel(
                b.Id,
                b.ChildPartId,
                b.ChildPart.PartNumber,
                b.ChildPart.Name,
                b.Quantity,
                b.ReferenceDesignator,
                b.SortOrder,
                b.SourceType,
                b.LeadTimeDays,
                b.Notes))
            .ToList();

        var usedIn = part.UsedInBOM
            .OrderBy(b => b.ParentPart.PartNumber)
            .Select(b => new BOMUsageResponseModel(
                b.Id,
                b.ParentPartId,
                b.ParentPart.PartNumber,
                b.ParentPart.Name,
                b.Quantity))
            .ToList();

        return new PartDetailResponseModel(
            part.Id,
            part.PartNumber,
            part.Name,
            part.Description,
            part.Revision,
            part.Status,
            part.PartType,
            // Pillar 1 — Decomposed type axes
            part.ProcurementSource,
            part.InventoryClass,
            part.ItemKindId,
            part.ItemKind?.Label,
            // Tier 0 additions
            part.TraceabilityType,
            part.AbcClass,
            part.ManufacturerName,
            part.ManufacturerPartNumber,
            part.Material,
            part.MoldToolRef,
            part.ExternalPartNumber,
            part.ExternalId,
            part.ExternalRef,
            part.Provider,
            part.PreferredVendorId,
            part.PreferredVendor?.CompanyName,
            part.MinStockThreshold,
            part.ReorderPoint,
            part.ReorderQuantity,
            part.LeadTimeDays,
            part.SafetyStockDays,
            part.IsSerialTracked,
            part.ToolingAssetId,
            part.ToolingAsset?.Name,
            // Workflow Pattern Phase 5 — surfaces cost gates for the hasCost predicate.
            part.ManualCostOverride,
            part.CurrentCostCalculationId,
            bomEntries,
            usedIn,
            part.CreatedAt,
            part.UpdatedAt);
    }

    public Task<Part?> FindAsync(int id, CancellationToken ct)
        => db.Parts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId, CancellationToken ct)
    {
        var query = db.Parts.Where(p => p.PartNumber == partNumber);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return query.AnyAsync(ct);
    }

    public async Task<string> GetNextPartNumberAsync(PartType partType, CancellationToken ct)
    {
        var prefix = partType switch
        {
            PartType.Part => "PRT-",
            PartType.Assembly => "ASM-",
            PartType.RawMaterial => "RAW-",
            PartType.Consumable => "CON-",
            PartType.Tooling => "TLG-",
            PartType.Fastener => "FST-",
            PartType.Electronic => "ELC-",
            PartType.Packaging => "PKG-",
            _ => "PRT-",
        };

        // IgnoreQueryFilters() so soft-deleted parts are also considered when
        // computing the next sequential number. The unique index ix_parts_part_number
        // covers ALL rows (including soft-deleted), so reusing a soft-deleted
        // suffix would 23505 the INSERT. (Phase 3 F6.)
        var suffixes = await db.Parts
            .IgnoreQueryFilters()
            .Where(p => p.PartNumber.StartsWith(prefix))
            .Select(p => p.PartNumber.Substring(prefix.Length))
            .ToListAsync(ct);

        var maxNumber = suffixes
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{maxNumber + 1:D5}";
    }

    public async Task AddAsync(Part part, CancellationToken ct)
    {
        await db.Parts.AddAsync(part, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<BOMEntry?> FindBomEntryAsync(int bomEntryId, int parentPartId, CancellationToken ct)
        => db.BOMEntries.FirstOrDefaultAsync(b => b.Id == bomEntryId && b.ParentPartId == parentPartId, ct);

    public async Task<int> GetMaxBomSortOrderAsync(int parentPartId, CancellationToken ct)
    {
        var max = await db.BOMEntries
            .Where(b => b.ParentPartId == parentPartId)
            .MaxAsync(b => (int?)b.SortOrder, ct);
        return max ?? 0;
    }

    public async Task AddBomEntryAsync(BOMEntry entry, CancellationToken ct)
    {
        await db.BOMEntries.AddAsync(entry, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task RemoveBomEntryAsync(BOMEntry entry)
    {
        db.BOMEntries.Remove(entry);
        return db.SaveChangesAsync(default);
    }

    public async Task<List<OperationResponseModel>> GetOperationsAsync(int partId, CancellationToken ct)
    {
        return await db.Operations
            .Where(s => s.PartId == partId)
            .Include(s => s.WorkCenter)
            .Include(s => s.ReferencedOperation)
            .Include(s => s.SubcontractVendor)
            .Include(s => s.Materials).ThenInclude(m => m.BomEntry).ThenInclude(b => b.ChildPart)
            .OrderBy(s => s.StepNumber)
            .Select(s => new OperationResponseModel(
                s.Id,
                s.PartId,
                s.StepNumber,
                s.Title,
                s.Instructions,
                s.WorkCenterId,
                s.WorkCenter != null ? s.WorkCenter.Name : null,
                s.EstimatedMinutes,
                s.IsQcCheckpoint,
                s.QcCriteria,
                s.ReferencedOperationId,
                s.ReferencedOperation != null ? $"Op {s.ReferencedOperation.StepNumber}: {s.ReferencedOperation.Title}" : null,
                s.Materials.Select(m => new OperationMaterialResponseModel(
                    m.Id,
                    m.OperationId,
                    m.BomEntryId,
                    m.BomEntry.ChildPart.PartNumber,
                    m.BomEntry.ChildPart.Name,
                    m.Quantity,
                    m.Notes)).ToList(),
                s.CreatedAt,
                s.UpdatedAt,
                // Phase 3 H5 / WU-13 — subcontract metadata round-tripped.
                s.IsSubcontract,
                s.SubcontractVendorId,
                s.SubcontractVendor != null ? s.SubcontractVendor.CompanyName : null,
                s.SubcontractTurnTimeDays))
            .ToListAsync(ct);
    }

    public Task<Operation?> FindOperationAsync(int operationId, CancellationToken ct)
        => db.Operations.FirstOrDefaultAsync(s => s.Id == operationId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
}
