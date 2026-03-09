using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IAssetRepository
{
    Task<List<AssetResponseModel>> GetAssetsAsync(AssetType? type, AssetStatus? status, string? search, CancellationToken ct);
    Task<AssetResponseModel?> GetByIdAsync(int id, CancellationToken ct);
    Task<Asset?> FindAsync(int id, CancellationToken ct);
    Task AddAsync(Asset asset, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
