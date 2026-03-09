using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ITrackTypeRepository
{
    Task<List<TrackTypeResponseModel>> GetAllAsync(CancellationToken ct);
    Task<TrackTypeResponseModel?> GetByIdAsync(int id, CancellationToken ct);
    Task<JobStage?> FindFirstActiveStageAsync(int trackTypeId, CancellationToken ct);
    Task<JobStage?> FindStageAsync(int stageId, CancellationToken ct);
    Task<TrackType?> FindAsync(int id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct);
    Task<int> GetMaxSortOrderAsync(CancellationToken ct);
    Task AddAsync(TrackType trackType, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
