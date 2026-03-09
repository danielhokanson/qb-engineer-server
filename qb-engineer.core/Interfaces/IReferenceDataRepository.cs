using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IReferenceDataRepository
{
    Task<List<ReferenceDataGroupResponseModel>> GetAllGroupsAsync(CancellationToken ct);
    Task<List<ReferenceDataResponseModel>> GetByGroupAsync(string groupCode, CancellationToken ct);
}
