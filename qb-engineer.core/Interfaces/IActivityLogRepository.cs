using QBEngineer.Core.Entities;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IActivityLogRepository
{
    Task<List<ActivityResponseModel>> GetByJobIdAsync(int jobId, CancellationToken ct);
    Task<bool> JobExistsAsync(int jobId, CancellationToken ct);
    Task AddAsync(JobActivityLog log, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
