using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IJobLinkRepository
{
    Task<List<JobLinkResponseModel>> GetByJobIdAsync(int jobId, CancellationToken ct);
    Task<JobLink?> FindAsync(int linkId, CancellationToken ct);
    Task<bool> JobExistsAsync(int jobId, CancellationToken ct);
    Task<bool> LinkExistsAsync(int sourceJobId, int targetJobId, JobLinkType linkType, CancellationToken ct);
    Task AddAsync(JobLink link, CancellationToken ct);
    Task RemoveAsync(JobLink link, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
