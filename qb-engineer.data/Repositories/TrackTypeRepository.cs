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
}
