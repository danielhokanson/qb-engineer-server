using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IPartRepository
{
    Task<List<PartListResponseModel>> GetPartsAsync(PartStatus? status, PartType? type, string? search, CancellationToken ct);
    Task<PartDetailResponseModel?> GetDetailAsync(int id, CancellationToken ct);
    Task<Part?> FindAsync(int id, CancellationToken ct);
    Task<bool> PartNumberExistsAsync(string partNumber, int? excludeId, CancellationToken ct);
    Task<string> GetNextPartNumberAsync(PartType partType, CancellationToken ct);
    Task AddAsync(Part part, CancellationToken ct);
    Task<BOMEntry?> FindBomEntryAsync(int bomEntryId, int parentPartId, CancellationToken ct);
    Task<int> GetMaxBomSortOrderAsync(int parentPartId, CancellationToken ct);
    Task AddBomEntryAsync(BOMEntry entry, CancellationToken ct);
    Task RemoveBomEntryAsync(BOMEntry entry);
    Task<List<OperationResponseModel>> GetOperationsAsync(int partId, CancellationToken ct);
    Task<Operation?> FindOperationAsync(int operationId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
