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
        var query = db.Parts.AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (type.HasValue)
            query = query.Where(p => p.PartType == type.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.PartNumber.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term) ||
                (p.Material != null && p.Material.ToLower().Contains(term)) ||
                (p.ExternalPartNumber != null && p.ExternalPartNumber.ToLower().Contains(term)));
        }

        var now = DateTimeOffset.UtcNow;

        var parts = await query
            .Include(p => p.BOMEntries)
            .OrderBy(p => p.PartNumber)
            .Select(p => new
            {
                Part = p,
                BomCount = p.BOMEntries.Count,
                CurrentPrice = db.PartPrices
                    .Where(pp => pp.PartId == p.Id && pp.EffectiveTo == null && pp.EffectiveFrom <= now)
                    .Select(pp => (decimal?)pp.UnitPrice)
                    .FirstOrDefault(),
            })
            .ToListAsync(ct);

        return parts.Select(r => new PartListResponseModel(
            r.Part.Id,
            r.Part.PartNumber,
            r.Part.Description,
            r.Part.Revision,
            r.Part.Status,
            r.Part.PartType,
            r.Part.Material,
            r.Part.ExternalPartNumber,
            r.BomCount,
            r.Part.CreatedAt,
            r.CurrentPrice
        )).ToList();
    }

    public async Task<PartDetailResponseModel?> GetDetailAsync(int id, CancellationToken ct)
    {
        var part = await db.Parts
            .Include(p => p.BOMEntries).ThenInclude(b => b.ChildPart)
            .Include(p => p.UsedInBOM).ThenInclude(b => b.ParentPart)
            .Include(p => p.PreferredVendor)
            .Include(p => p.ToolingAsset)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (part is null)
            return null;

        var bomEntries = part.BOMEntries
            .OrderBy(b => b.SortOrder)
            .Select(b => new BOMEntryResponseModel(
                b.Id,
                b.ChildPartId,
                b.ChildPart.PartNumber,
                b.ChildPart.Description,
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
                b.ParentPart.Description,
                b.Quantity))
            .ToList();

        return new PartDetailResponseModel(
            part.Id,
            part.PartNumber,
            part.Description,
            part.Revision,
            part.Status,
            part.PartType,
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
            part.ToolingAssetId,
            part.ToolingAsset?.Name,
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

        var suffixes = await db.Parts
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

    public async Task<List<ProcessStepResponseModel>> GetProcessStepsAsync(int partId, CancellationToken ct)
    {
        return await db.ProcessSteps
            .Where(s => s.PartId == partId)
            .Include(s => s.WorkCenter)
            .OrderBy(s => s.StepNumber)
            .Select(s => new ProcessStepResponseModel(
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
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync(ct);
    }

    public Task<ProcessStep?> FindProcessStepAsync(int stepId, CancellationToken ct)
        => db.ProcessSteps.FirstOrDefaultAsync(s => s.Id == stepId, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => db.SaveChangesAsync(ct);
}
