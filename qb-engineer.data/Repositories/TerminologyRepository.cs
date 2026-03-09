using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class TerminologyRepository(AppDbContext db) : ITerminologyRepository
{
    public async Task<List<TerminologyEntryResponseModel>> GetAllAsync(CancellationToken ct)
    {
        return await db.TerminologyEntries
            .OrderBy(t => t.Key)
            .Select(t => new TerminologyEntryResponseModel(t.Key, t.Label))
            .ToListAsync(ct);
    }

    public async Task<TerminologyEntry?> FindByKeyAsync(string key, CancellationToken ct)
    {
        return await db.TerminologyEntries
            .FirstOrDefaultAsync(t => t.Key == key, ct);
    }

    public async Task AddAsync(TerminologyEntry entry, CancellationToken ct)
    {
        await db.TerminologyEntries.AddAsync(entry, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
