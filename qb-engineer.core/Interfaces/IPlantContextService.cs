using QBEngineer.Core.Entities;

namespace QBEngineer.Core.Interfaces;

public interface IPlantContextService
{
    int? CurrentPlantId { get; }
    void SetPlant(int plantId);
    Task<IReadOnlyList<Plant>> GetUserPlantsAsync(int userId, CancellationToken ct);
}
