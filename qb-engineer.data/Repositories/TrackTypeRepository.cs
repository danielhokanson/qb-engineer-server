using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class TrackTypeRepository(AppDbContext db) : ITrackTypeRepository
{
    public async Task<List<TrackTypeResponseModel>> GetAllAsync(CancellationToken ct)
    {
        return await db.TrackTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => new TrackTypeResponseModel(
                t.Id,
                t.Name,
                t.Code,
                t.Description,
                t.IsDefault,
                t.SortOrder,
                t.Stages
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new StageResponseModel(
                        s.Id,
                        s.Name,
                        s.Code,
                        s.SortOrder,
                        s.Color,
                        s.WIPLimit,
                        s.AccountingDocumentType != null ? s.AccountingDocumentType.ToString() : null,
                        s.IsIrreversible))
                    .ToList()))
            .ToListAsync(ct);
    }

    public async Task<TrackTypeResponseModel?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.TrackTypes
            .Where(t => t.Id == id && t.IsActive)
            .Select(t => new TrackTypeResponseModel(
                t.Id,
                t.Name,
                t.Code,
                t.Description,
                t.IsDefault,
                t.SortOrder,
                t.Stages
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new StageResponseModel(
                        s.Id,
                        s.Name,
                        s.Code,
                        s.SortOrder,
                        s.Color,
                        s.WIPLimit,
                        s.AccountingDocumentType != null ? s.AccountingDocumentType.ToString() : null,
                        s.IsIrreversible))
                    .ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<JobStage?> FindFirstActiveStageAsync(int trackTypeId, CancellationToken ct)
    {
        return await db.JobStages
            .Where(s => s.TrackTypeId == trackTypeId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<JobStage?> FindStageAsync(int stageId, CancellationToken ct)
    {
        return await db.JobStages.FirstOrDefaultAsync(s => s.Id == stageId, ct);
    }

    public async Task<List<JobStage>> GetStagesByTrackTypeAsync(int trackTypeId, CancellationToken ct)
    {
        return await db.JobStages
            .Where(s => s.TrackTypeId == trackTypeId && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<TrackType?> FindAsync(int id, CancellationToken ct)
    {
        return await db.TrackTypes
            .Include(t => t.Stages)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct)
    {
        return await db.TrackTypes
            .AnyAsync(t => t.Code == code && t.IsActive && (!excludeId.HasValue || t.Id != excludeId.Value), ct);
    }

    public async Task<int> GetMaxSortOrderAsync(CancellationToken ct)
    {
        return await db.TrackTypes
            .Where(t => t.IsActive)
            .MaxAsync(t => (int?)t.SortOrder, ct) ?? 0;
    }

    public async Task AddAsync(TrackType trackType, CancellationToken ct)
    {
        db.TrackTypes.Add(trackType);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
