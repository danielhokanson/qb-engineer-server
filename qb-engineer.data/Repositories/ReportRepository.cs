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

    public async Task<OnTimeDeliveryReportItem> GetOnTimeDeliveryAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        var startUtc = start.UtcDateTime;
        var endUtc = end.UtcDateTime;

        var completedJobs = await db.Jobs
            .Where(j => j.CompletedDate.HasValue
                && j.CompletedDate.Value >= startUtc
                && j.CompletedDate.Value <= endUtc
                && j.DueDate.HasValue)
            .Select(j => new { j.DueDate, j.CompletedDate })
            .ToListAsync(ct);

        var total = completedJobs.Count;
        var onTime = completedJobs.Count(j => j.CompletedDate!.Value.Date <= j.DueDate!.Value.Date);
        var late = total - onTime;
        var percent = total > 0 ? Math.Round((decimal)onTime / total * 100, 1) : 0;

        return new OnTimeDeliveryReportItem(total, onTime, late, percent);
    }

    public async Task<List<AverageLeadTimeReportItem>> GetAverageLeadTimeAsync(CancellationToken ct)
    {
        // Calculate average time jobs spend at each stage based on activity log stage moves
        var stageMoves = await db.Set<QBEngineer.Core.Entities.JobActivityLog>()
            .Where(a => a.Action == QBEngineer.Core.Enums.ActivityAction.StageMoved)
            .OrderBy(a => a.JobId).ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

        var stageTimesRaw = new Dictionary<string, List<double>>();
        var byJob = stageMoves.GroupBy(a => a.JobId);
        foreach (var group in byJob)
        {
            var moves = group.OrderBy(m => m.CreatedAt).ToList();
            for (var i = 0; i < moves.Count - 1; i++)
            {
                var stageName = ExtractStageName(moves[i].Description);
                if (stageName is null) continue;
                var days = (moves[i + 1].CreatedAt - moves[i].CreatedAt).TotalDays;
                if (!stageTimesRaw.ContainsKey(stageName))
                    stageTimesRaw[stageName] = [];
                stageTimesRaw[stageName].Add(days);
            }
        }

        var stages = await db.Set<QBEngineer.Core.Entities.JobStage>()
            .Select(s => new { s.Name, s.Color })
            .Distinct()
            .ToListAsync(ct);
        var colorMap = stages.ToDictionary(s => s.Name, s => s.Color ?? "#94a3b8");

        return stageTimesRaw
            .Select(kv => new AverageLeadTimeReportItem(
                kv.Key,
                colorMap.GetValueOrDefault(kv.Key, "#94a3b8"),
                Math.Round((decimal)kv.Value.Average(), 1)))
            .OrderBy(r => r.StageName)
            .ToList();
    }

    public async Task<List<TeamWorkloadReportItem>> GetTeamWorkloadAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday) startOfWeek = startOfWeek.AddDays(-7);
        var weekStart = DateOnly.FromDateTime(startOfWeek);
        var weekEnd = DateOnly.FromDateTime(today);

        var assignedJobs = await db.Jobs
            .Where(j => !j.IsArchived && j.AssigneeId.HasValue)
            .GroupBy(j => j.AssigneeId!.Value)
            .Select(g => new
            {
                UserId = g.Key,
                ActiveJobs = g.Count(j => j.CompletedDate == null),
                OverdueJobs = g.Count(j => j.CompletedDate == null && j.DueDate.HasValue && j.DueDate.Value < today),
            })
            .ToListAsync(ct);

        var userIds = assignedJobs.Select(j => j.UserId).ToList();

        var users = await db.Users
            .Where(u => userIds.Contains(u.Id) && u.IsActive)
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim(), u.Initials, u.AvatarColor })
            .ToDictionaryAsync(u => u.Id, ct);

        var weeklyHours = await db.TimeEntries
            .Where(t => userIds.Contains(t.UserId) && t.Date >= weekStart && t.Date <= weekEnd)
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, Minutes = g.Sum(t => t.DurationMinutes) })
            .ToDictionaryAsync(g => g.UserId, g => g.Minutes, ct);

        return assignedJobs
            .Where(j => users.ContainsKey(j.UserId))
            .Select(j =>
            {
                var user = users[j.UserId];
                var hours = Math.Round((decimal)weeklyHours.GetValueOrDefault(j.UserId, 0) / 60, 1);
                return new TeamWorkloadReportItem(
                    j.UserId, user.Name, user.Initials ?? "??", user.AvatarColor ?? "#94a3b8",
                    j.ActiveJobs, j.OverdueJobs, hours);
            })
            .OrderByDescending(r => r.ActiveJobs)
            .ToList();
    }

    public async Task<List<CustomerActivityReportItem>> GetCustomerActivityAsync(CancellationToken ct)
    {
        return await db.Jobs
            .Where(j => j.CustomerId.HasValue)
            .GroupBy(j => new { j.CustomerId, j.Customer!.Name })
            .Select(g => new CustomerActivityReportItem(
                g.Key.CustomerId!.Value,
                g.Key.Name,
                g.Count(j => !j.IsArchived && j.CompletedDate == null),
                g.Count(j => j.CompletedDate != null),
                g.Count(),
                g.Max(j => j.CreatedAt)))
            .OrderByDescending(r => r.TotalJobs)
            .ToListAsync(ct);
    }

    public async Task<List<MyWorkHistoryReportItem>> GetMyWorkHistoryAsync(int userId, CancellationToken ct)
    {
        return await db.Jobs
            .Include(j => j.CurrentStage)
            .Include(j => j.Customer)
            .Where(j => j.AssigneeId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new MyWorkHistoryReportItem(
                j.Id,
                j.JobNumber,
                j.Title,
                j.CurrentStage.Name,
                j.CurrentStage.Color,
                j.Customer != null ? j.Customer.Name : null,
                j.DueDate,
                j.CreatedAt,
                j.CompletedDate))
            .ToListAsync(ct);
    }

    public async Task<List<MyTimeLogReportItem>> GetMyTimeLogAsync(int userId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        var startDate = DateOnly.FromDateTime(start.UtcDateTime);
        var endDate = DateOnly.FromDateTime(end.UtcDateTime);

        return await db.TimeEntries
            .Include(t => t.Job)
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .Select(t => new MyTimeLogReportItem(
                t.Id,
                t.Job != null ? t.Job.JobNumber : null,
                t.Job != null ? t.Job.Title : null,
                t.Notes,
                t.DurationMinutes,
                t.Category,
                t.Date))
            .ToListAsync(ct);
    }

    private static string? ExtractStageName(string description)
    {
        var prefix = "Moved to ";
        if (description.StartsWith(prefix))
            return description[prefix.Length..].Trim();
        return null;
    }
}
