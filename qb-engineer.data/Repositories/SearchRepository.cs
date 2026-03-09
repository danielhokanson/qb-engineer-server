using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class SearchRepository(AppDbContext db) : ISearchRepository
{
    public async Task<List<SearchResultModel>> SearchAsync(string term, int limit, CancellationToken ct)
    {
        var perEntity = Math.Max(limit / 4, 3);
        var results = new List<SearchResultModel>();

        var pattern = $"%{term}%";

        // Jobs
        var jobs = await db.Jobs
            .Where(j => !j.IsArchived &&
                (EF.Functions.ILike(j.Title, pattern) || EF.Functions.ILike(j.JobNumber, pattern)))
            .OrderByDescending(j => EF.Functions.ILike(j.JobNumber, pattern))
            .ThenByDescending(j => j.UpdatedAt)
            .Take(perEntity)
            .Select(j => new SearchResultModel("Job", j.Id, j.JobNumber, j.Title, "work", "/kanban"))
            .ToListAsync(ct);
        results.AddRange(jobs);

        // Customers
        var customers = await db.Customers
            .Where(c => EF.Functions.ILike(c.Name, pattern) ||
                (c.CompanyName != null && EF.Functions.ILike(c.CompanyName, pattern)) ||
                (c.Email != null && EF.Functions.ILike(c.Email, pattern)))
            .Take(perEntity)
            .Select(c => new SearchResultModel("Customer", c.Id, c.Name, c.CompanyName, "people", "/customers"))
            .ToListAsync(ct);
        results.AddRange(customers);

        // Parts
        var parts = await db.Parts
            .Where(p => EF.Functions.ILike(p.PartNumber, pattern) ||
                (p.Description != null && EF.Functions.ILike(p.Description, pattern)))
            .Take(perEntity)
            .Select(p => new SearchResultModel("Part", p.Id, p.PartNumber, p.Description, "inventory_2", "/parts"))
            .ToListAsync(ct);
        results.AddRange(parts);

        // Leads
        var leads = await db.Leads
            .Where(l => EF.Functions.ILike(l.CompanyName, pattern) ||
                (l.ContactName != null && EF.Functions.ILike(l.ContactName, pattern)) ||
                (l.Email != null && EF.Functions.ILike(l.Email, pattern)))
            .Take(perEntity)
            .Select(l => new SearchResultModel("Lead", l.Id, l.CompanyName, l.ContactName, "trending_up", "/leads"))
            .ToListAsync(ct);
        results.AddRange(leads);

        // Assets
        var assets = await db.Assets
            .Where(a => EF.Functions.ILike(a.Name, pattern) ||
                (a.SerialNumber != null && EF.Functions.ILike(a.SerialNumber, pattern)))
            .Take(perEntity)
            .Select(a => new SearchResultModel("Asset", a.Id, a.Name, a.SerialNumber, "precision_manufacturing", "/assets"))
            .ToListAsync(ct);
        results.AddRange(assets);

        // Expenses
        var expenses = await db.Expenses
            .Where(e => EF.Functions.ILike(e.Description, pattern) || EF.Functions.ILike(e.Category, pattern))
            .Take(perEntity)
            .Select(e => new SearchResultModel("Expense", e.Id, e.Description, e.Category, "receipt_long", "/expenses"))
            .ToListAsync(ct);
        results.AddRange(expenses);

        return results.Take(limit).ToList();
    }
}
