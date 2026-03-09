using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IInventoryRepository
{
    // Locations
    Task<List<StorageLocationResponseModel>> GetLocationTreeAsync(CancellationToken ct);
    Task<List<StorageLocationFlatResponseModel>> GetBinLocationsAsync(CancellationToken ct);
    Task<StorageLocation?> FindLocationAsync(int id, CancellationToken ct);
    Task<bool> BarcodeExistsAsync(string barcode, int? excludeId, CancellationToken ct);
    Task AddLocationAsync(StorageLocation location, CancellationToken ct);

    // Bin contents
    Task<List<BinContentResponseModel>> GetBinContentsAsync(int locationId, CancellationToken ct);
    Task<BinContent?> FindBinContentAsync(int id, CancellationToken ct);
    Task AddBinContentAsync(BinContent content, CancellationToken ct);
    Task AddMovementAsync(BinMovement movement, CancellationToken ct);

    // Inventory summary
    Task<List<InventoryPartSummaryResponseModel>> GetPartInventorySummaryAsync(string? search, CancellationToken ct);

    // Movement history
    Task<List<BinMovementResponseModel>> GetMovementsAsync(int? locationId, string? entityType, int? entityId, int take, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
