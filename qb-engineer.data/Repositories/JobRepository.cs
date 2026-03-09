using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Data.Repositories;

public class JobRepository(AppDbContext db) : IJobRepository
{
    public async Task<List<JobListResponseModel>> GetJobsAsync(
        int? trackTypeId, int? stageId, int? assigneeId,
        bool isArchived, string? search, CancellationToken ct)
    {
        var query = db.Jobs
            .Include(j => j.CurrentStage)
            .Include(j => j.Customer)
            .Where(j => j.IsArchived == isArchived);

        if (trackTypeId.HasValue)
            query = query.Where(j => j.TrackTypeId == trackTypeId.Value);

        if (stageId.HasValue)
            query = query.Where(j => j.CurrentStageId == stageId.Value);

        if (assigneeId.HasValue)
            query = query.Where(j => j.AssigneeId == assigneeId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(term) ||
                j.JobNumber.ToLower().Contains(term));
        }

        var jobs = await query
            .OrderBy(j => j.CurrentStage.SortOrder)
            .ThenBy(j => j.BoardPosition)
            .ToListAsync(ct);

        // Load assignee info separately (ApplicationUser is in data layer)
        var assigneeIds = jobs
            .Where(j => j.AssigneeId.HasValue)
            .Select(j => j.AssigneeId!.Value)
            .Distinct()
            .ToList();

        var assignees = assigneeIds.Count > 0
            ? await db.Users
                .Where(u => assigneeIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, ct)
            : [];

        return jobs.Select(j =>
        {
            var assignee = j.AssigneeId.HasValue && assignees.TryGetValue(j.AssigneeId.Value, out var u) ? u : null;
            return new JobListResponseModel(
                j.Id,
                j.JobNumber,
                j.Title,
                j.CurrentStage.Name,
                j.CurrentStage.Color,
                assignee?.Initials,
                assignee?.AvatarColor,
                j.Priority.ToString(),
                j.DueDate,
                j.DueDate.HasValue && j.DueDate.Value < DateTime.UtcNow && j.CompletedDate == null,
                j.Customer?.Name);
        }).ToList();
    }

    public async Task<JobDetailResponseModel?> GetDetailAsync(int id, CancellationToken ct)
    {
        var job = await db.Jobs
            .Include(j => j.CurrentStage)
            .Include(j => j.TrackType)
            .Include(j => j.Customer)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

        if (job is null) return null;

        ApplicationUser? assignee = null;
        if (job.AssigneeId.HasValue)
            assignee = await db.Users.FirstOrDefaultAsync(u => u.Id == job.AssigneeId.Value, ct);

        return new JobDetailResponseModel(
            job.Id,
            job.JobNumber,
            job.Title,
            job.Description,
            job.TrackTypeId,
            job.TrackType.Name,
            job.CurrentStageId,
            job.CurrentStage.Name,
            job.CurrentStage.Color,
            job.AssigneeId,
            assignee?.Initials,
            assignee is not null ? $"{assignee.FirstName} {assignee.LastName}".Trim() : null,
            assignee?.AvatarColor,
            job.Priority.ToString(),
            job.CustomerId,
            job.Customer?.Name,
            job.DueDate,
            job.StartDate,
            job.CompletedDate,
            job.IsArchived,
            job.BoardPosition,
            job.CreatedAt,
            job.UpdatedAt);
    }

    public async Task<Job?> FindAsync(int id, CancellationToken ct)
    {
        return await db.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task<string> GenerateNextJobNumberAsync(CancellationToken ct)
    {
        var maxJobNumber = await db.Jobs
            .Select(j => j.JobNumber)
            .OrderByDescending(jn => jn)
            .FirstOrDefaultAsync(ct);

        if (maxJobNumber is not null && maxJobNumber.StartsWith("J-")
            && int.TryParse(maxJobNumber[2..], out var currentNumber))
        {
            return $"J-{currentNumber + 1}";
        }

        return "J-1001";
    }

    public async Task<int> GetMaxBoardPositionAsync(int stageId, CancellationToken ct)
    {
        return await db.Jobs
            .Where(j => j.CurrentStageId == stageId)
            .MaxAsync(j => (int?)j.BoardPosition, ct) ?? 0;
    }

    public async Task<List<Job>> FindMultipleAsync(List<int> ids, CancellationToken ct)
    {
        return await db.Jobs
            .Include(j => j.CurrentStage)
            .Where(j => ids.Contains(j.Id))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Job job, CancellationToken ct)
    {
        db.Jobs.Add(job);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);
    }
}
