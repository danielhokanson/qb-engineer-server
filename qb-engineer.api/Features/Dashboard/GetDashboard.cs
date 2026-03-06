using MediatR;
using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Dashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardHandler(AppDbContext db) : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Load the production track type (default) with stages
        var productionTrack = await db.TrackTypes
            .Include(t => t.Stages.Where(s => s.IsActive))
            .Where(t => t.IsDefault && t.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (productionTrack is null)
        {
            return EmptyDashboard();
        }

        var stages = productionTrack.Stages
            .OrderBy(s => s.SortOrder)
            .ToList();

        var totalStages = stages.Count;

        // Load active (non-archived) jobs for this track type
        var jobs = await db.Jobs
            .Include(j => j.CurrentStage)
            .Include(j => j.Customer)
            .Where(j => j.TrackTypeId == productionTrack.Id && !j.IsArchived)
            .OrderBy(j => j.CurrentStage.SortOrder)
            .ThenBy(j => j.BoardPosition)
            .ToListAsync(cancellationToken);

        // Load assignee info for jobs that have one
        var assigneeIds = jobs
            .Where(j => j.AssigneeId.HasValue)
            .Select(j => j.AssigneeId!.Value)
            .Distinct()
            .ToList();

        var users = await db.Users
            .Where(u => assigneeIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        // Build task list
        var tasks = jobs.Select((job, index) => {
            var stage = job.CurrentStage;
            var stageIndex = stages.FindIndex(s => s.Id == stage.Id);
            var (status, statusColor) = DeriveStatus(stageIndex, totalStages, job.DueDate, today);
            var assignee = GetAssigneeInfo(job.AssigneeId, users);
            var time = GenerateDisplayTime(index);

            return new DashboardTaskDto(
                time,
                job.Title,
                job.JobNumber,
                stage.Color,
                assignee,
                status,
                statusColor);
        }).ToList();

        // Stage counts
        var jobsByStage = jobs.GroupBy(j => j.CurrentStageId).ToDictionary(g => g.Key, g => g.Count());
        var maxJobsPerStage = jobsByStage.Values.DefaultIfEmpty(0).Max();
        var stageCounts = stages.Select(s => new StageCountDto(
            s.Name,
            jobsByStage.GetValueOrDefault(s.Id, 0),
            s.Color,
            Math.Max(maxJobsPerStage, 1)
        )).ToList();

        // Team members
        var teamMembers = jobs
            .Where(j => j.AssigneeId.HasValue)
            .GroupBy(j => j.AssigneeId!.Value)
            .Select(g =>
            {
                var user = users.GetValueOrDefault(g.Key);
                return new TeamMemberDto(
                    user?.Initials ?? "??",
                    user is not null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown",
                    user?.AvatarColor ?? "#94a3b8",
                    g.Count(),
                    Math.Max(g.Count(), 5));
            })
            .OrderByDescending(t => t.TaskCount)
            .ToList();

        // Activity log - last 5 entries
        var activityLogs = await db.JobActivityLogs
            .Include(a => a.Job)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var activityUserIds = activityLogs
            .Where(a => a.UserId.HasValue)
            .Select(a => a.UserId!.Value)
            .Distinct()
            .ToList();

        var activityUsers = await db.Users
            .Where(u => activityUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var activity = activityLogs.Select(a =>
        {
            var (icon, iconColor) = GetActivityIcon(a.Action);
            var userName = a.UserId.HasValue && activityUsers.TryGetValue(a.UserId.Value, out var user)
                ? user.FirstName
                : "System";
            var text = $"{userName} {a.Description}";
            var time = FormatRelativeTime(a.CreatedAt, now);

            return new ActivityEntryDto(icon, iconColor, text, time);
        }).ToList();

        // Deadlines - jobs due in next 14 days
        var deadlineCutoff = today.AddDays(14);
        var deadlines = jobs
            .Where(j => j.DueDate.HasValue && j.DueDate.Value.Date <= deadlineCutoff)
            .OrderBy(j => j.DueDate)
            .Select(j => new DeadlineEntryDto(
                j.DueDate!.Value.ToString("MMM d"),
                j.JobNumber,
                j.Title,
                j.DueDate.Value.Date < today))
            .ToList();

        // KPIs
        var activeCount = jobs.Count;
        var overdueCount = jobs.Count(j => j.DueDate.HasValue && j.DueDate.Value.Date < today);

        var kpis = new DashboardKPIsDto(
            activeCount,
            0, // Change tracking requires historical comparison; hardcoded for now
            overdueCount,
            0,
            "0h",
            "neutral");

        return new DashboardDto(tasks, stageCounts, teamMembers, activity, deadlines, kpis);
    }

    private static DashboardDto EmptyDashboard() => new([], [], [], [], [],
        new DashboardKPIsDto(0, 0, 0, 0, "0h", "neutral"));

    private static string GenerateDisplayTime(int index)
    {
        // Generate times starting at 8:00a in 30-minute increments
        var totalMinutes = 8 * 60 + index * 30;
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        var period = hours >= 12 ? "p" : "a";
        var displayHour = hours > 12 ? hours - 12 : hours;
        if (displayHour == 0) displayHour = 12;

        return minutes == 0
            ? $"{displayHour}:{minutes:D2}{period}"
            : $"{displayHour}:{minutes:D2}{period}";
    }

    private static (string Status, string StatusColor) DeriveStatus(int stageIndex, int totalStages, DateTime? dueDate, DateTime today)
    {
        if (dueDate.HasValue && dueDate.Value.Date < today)
            return ("LATE", "#ef4444");

        if (totalStages <= 1)
            return ("ACTIVE", "#3b82f6");

        // First third of stages = NEXT, middle = ACTIVE, last third = FINISHING
        var position = (double)stageIndex / (totalStages - 1);

        return position switch
        {
            < 0.34 => ("NEXT", "#94a3b8"),
            < 0.67 => ("ACTIVE", "#3b82f6"),
            _ => ("FINISHING", "#22c55e")
        };
    }

    private static AssigneeInfo GetAssigneeInfo(int? assigneeId, Dictionary<int, ApplicationUser> users)
    {
        if (assigneeId.HasValue && users.TryGetValue(assigneeId.Value, out var user))
            return new AssigneeInfo(user.Initials ?? "??", user.AvatarColor ?? "#94a3b8");

        return new AssigneeInfo("--", "#cbd5e1");
    }

    private static (string Icon, string IconColor) GetActivityIcon(ActivityAction action) => action switch
    {
        ActivityAction.Created => ("add_circle", "#22c55e"),
        ActivityAction.StageMoved => ("swap_horiz", "#3b82f6"),
        ActivityAction.FieldChanged => ("edit", "#f59e0b"),
        ActivityAction.Assigned => ("person_add", "#8b5cf6"),
        ActivityAction.Unassigned => ("person_remove", "#94a3b8"),
        ActivityAction.SubtaskAdded => ("checklist", "#06b6d4"),
        ActivityAction.SubtaskCompleted => ("task_alt", "#22c55e"),
        ActivityAction.CommentAdded => ("comment", "#6366f1"),
        ActivityAction.Archived => ("archive", "#64748b"),
        ActivityAction.Restored => ("unarchive", "#f97316"),
        _ => ("info", "#94a3b8")
    };

    private static string FormatRelativeTime(DateTime timestamp, DateTime now)
    {
        var diff = now - timestamp;

        return diff.TotalMinutes switch
        {
            < 1 => "just now",
            < 60 => $"{(int)diff.TotalMinutes}m ago",
            < 1440 => $"{(int)diff.TotalHours}h ago",
            < 10080 => $"{(int)diff.TotalDays}d ago",
            _ => timestamp.ToString("MMM d")
        };
    }
}
