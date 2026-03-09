using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class ReportRepository(AppDbContext db) : IReportRepository
{
    public async Task<List<JobsByStageReportItem>> GetJobsByStageAsync(int? trackTypeId, CancellationToken ct)
    {
        var query = db.Jobs
            .Include(j => j.CurrentStage)
            .Where(j => !j.IsArchived);

        if (trackTypeId.HasValue)
            query = query.Where(j => j.TrackTypeId == trackTypeId.Value);

        return await query
            .GroupBy(j => new { j.CurrentStage.Name, j.CurrentStage.Color, j.CurrentStage.SortOrder })
            .Select(g => new JobsByStageReportItem(g.Key.Name, g.Key.Color, g.Count()))
            .OrderBy(r => r.StageName)
            .ToListAsync(ct);
    }

    public async Task<List<OverdueJobReportItem>> GetOverdueJobsAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var overdueJobs = await db.Jobs
            .Where(j => !j.IsArchived && j.DueDate.HasValue && j.DueDate.Value < today)
            .OrderBy(j => j.DueDate)
            .Select(j => new { j.Id, j.JobNumber, j.Title, j.DueDate, j.AssigneeId })
            .ToListAsync(ct);

        var assigneeIds = overdueJobs.Where(j => j.AssigneeId.HasValue).Select(j => j.AssigneeId!.Value).Distinct().ToList();
        var users = assigneeIds.Count > 0
            ? await db.Users.Where(u => assigneeIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), ct)
            : new Dictionary<int, string>();

        return overdueJobs.Select(j => new OverdueJobReportItem(
            j.Id,
            j.JobNumber,
            j.Title,
            j.DueDate!.Value,
            (int)(today - j.DueDate.Value).TotalDays,
            j.AssigneeId.HasValue ? users.GetValueOrDefault(j.AssigneeId.Value) : null)).ToList();
    }

    public async Task<List<TimeByUserReportItem>> GetTimeByUserAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        var startDate = DateOnly.FromDateTime(start.UtcDateTime.Date);
        var endDate = DateOnly.FromDateTime(end.UtcDateTime.Date);

        return await db.TimeEntries
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => new { t.UserId })
            .Select(g => new TimeByUserReportItem(
                g.Key.UserId,
                "", // populated in handler with user lookup
                Math.Round((decimal)g.Sum(t => t.DurationMinutes) / 60, 1)))
            .OrderByDescending(r => r.TotalHours)
            .ToListAsync(ct);
    }

    public async Task<List<ExpenseSummaryReportItem>> GetExpenseSummaryAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        var startUtc = start.UtcDateTime;
        var endUtc = end.UtcDateTime;

        return await db.Expenses
            .Where(e => e.ExpenseDate >= startUtc && e.ExpenseDate <= endUtc)
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseSummaryReportItem(
                g.Key,
                g.Sum(e => e.Amount),
                g.Count()))
            .OrderByDescending(r => r.TotalAmount)
            .ToListAsync(ct);
    }

    public async Task<List<LeadPipelineReportItem>> GetLeadPipelineAsync(CancellationToken ct)
    {
        return await db.Leads
            .GroupBy(l => l.Status)
            .Select(g => new LeadPipelineReportItem(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<List<JobCompletionTrendItem>> GetJobCompletionTrendAsync(int months, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-months);

        var created = await db.Jobs
            .Where(j => j.CreatedAt >= cutoff)
            .GroupBy(j => new { j.CreatedAt.Year, j.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var completed = await db.Jobs
            .Where(j => j.CompletedDate.HasValue && j.CompletedDate.Value >= cutoff)
            .GroupBy(j => new { j.CompletedDate!.Value.Year, j.CompletedDate!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        var result = new List<JobCompletionTrendItem>();
        for (var i = months - 1; i >= 0; i--)
        {
            var d = DateTime.UtcNow.AddMonths(-i);
            var monthLabel = d.ToString("MMM yyyy");
            var createdCount = created.FirstOrDefault(c => c.Year == d.Year && c.Month == d.Month)?.Count ?? 0;
            var completedCount = completed.FirstOrDefault(c => c.Year == d.Year && c.Month == d.Month)?.Count ?? 0;
            result.Add(new JobCompletionTrendItem(monthLabel, createdCount, completedCount));
        }

        return result;
    }
}
