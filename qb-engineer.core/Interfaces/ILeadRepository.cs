using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ILeadRepository
{
    Task<List<LeadResponseModel>> GetLeadsAsync(LeadStatus? status, string? search, CancellationToken ct);
    Task<LeadResponseModel?> GetByIdAsync(int id, CancellationToken ct);
    Task<Lead?> FindAsync(int id, CancellationToken ct);
    Task AddAsync(Lead lead, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
