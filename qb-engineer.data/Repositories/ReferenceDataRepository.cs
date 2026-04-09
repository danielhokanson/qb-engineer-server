using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class ReferenceDataRepository(AppDbContext db) : IReferenceDataRepository
{
    public async Task<List<ReferenceDataGroupResponseModel>> GetAllGroupsAsync(CancellationToken ct)
    {
        var allData = await db.ReferenceData
            .OrderBy(r => r.GroupCode)
            .ThenBy(r => r.SortOrder)
            .ToListAsync(ct);

        return allData
            .GroupBy(r => r.GroupCode)
            .Select(g => new ReferenceDataGroupResponseModel(
                g.Key,
                g.Select(r => new ReferenceDataResponseModel(
                    r.Id, r.Code, r.Label, r.SortOrder, r.IsActive, r.IsSeedData, r.Metadata))
                .ToList()))
            .ToList();
    }

    public async Task<List<ReferenceDataResponseModel>> GetByGroupAsync(string groupCode, CancellationToken ct)
    {
        return await db.ReferenceData
            .Where(r => r.GroupCode == groupCode)
            .OrderBy(r => r.SortOrder)
            .Select(r => new ReferenceDataResponseModel(
                r.Id, r.Code, r.Label, r.SortOrder, r.IsActive, r.IsSeedData, r.Metadata))
            .ToListAsync(ct);
    }
}
