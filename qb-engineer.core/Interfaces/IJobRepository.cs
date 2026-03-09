using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IJobRepository
{
    Task<List<JobListResponseModel>> GetJobsAsync(int? trackTypeId, int? stageId, int? assigneeId, bool isArchived, string? search, CancellationToken ct);
    Task<JobDetailResponseModel?> GetDetailAsync(int id, CancellationToken ct);
    Task<Job?> FindAsync(int id, CancellationToken ct);
    Task<string> GenerateNextJobNumberAsync(CancellationToken ct);
    Task<int> GetMaxBoardPositionAsync(int stageId, CancellationToken ct);
    Task AddAsync(Job job, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
