using Microsoft.EntityFrameworkCore;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Jobs;

public class DailyDigestJob(
    AppDbContext db,
    IEmailService emailService,
    ISystemSettingRepository settings,
    ILogger<DailyDigestJob> logger)
{
    public async Task SendDailyDigestAsync()
    {
        var companySetting = await settings.FindByKeyAsync("company_name", CancellationToken.None);
        var companyName = companySetting?.Value ?? "QB Engineer";

        // Get users with email digest enabled
        var users = await db.Users
            .Where(u => u.IsActive && u.Email != null)
            .Select(u => new { u.Id, u.Email, u.FirstName })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        foreach (var user in users)
        {
            try
            {
                // Jobs assigned to user due in next 3 days
                var upcomingJobs = await db.Jobs
                    .Where(j => j.AssigneeId == user.Id && !j.IsArchived
                        && j.DueDate.HasValue && j.DueDate.Value <= now.AddDays(3)
                        && j.CompletedDate == null)
                    .OrderBy(j => j.DueDate)
                    .Select(j => new { j.JobNumber, j.Title, j.DueDate })
                    .Take(10)
                    .ToListAsync();

                // Overdue jobs
                var overdueJobs = await db.Jobs
                    .Where(j => j.AssigneeId == user.Id && !j.IsArchived
                        && j.DueDate.HasValue && j.DueDate.Value < now
                        && j.CompletedDate == null)
                    .Select(j => new { j.JobNumber, j.Title, j.DueDate })
                    .Take(10)
                    .ToListAsync();

                // Jobs completed yesterday
                var completedYesterday = await db.Jobs
                    .Where(j => j.AssigneeId == user.Id
                        && j.CompletedDate.HasValue && j.CompletedDate.Value >= yesterday)
                    .CountAsync();

                if (upcomingJobs.Count == 0 && overdueJobs.Count == 0 && completedYesterday == 0)
                    continue;

                var html = EmailTemplateBuilder.BuildDigest(
                    companyName,
                    user.FirstName ?? "Team Member",
                    upcomingJobs.Select(j => new DigestJobItem(j.JobNumber, j.Title, j.DueDate)).ToList(),
                    overdueJobs.Select(j => new DigestJobItem(j.JobNumber, j.Title, j.DueDate)).ToList(),
                    completedYesterday);

                await emailService.SendAsync(new EmailMessage(
                    user.Email!,
                    $"[{companyName}] Daily Digest — {now:MMMM d, yyyy}",
                    html));

                logger.LogInformation("Daily digest sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send daily digest to {Email}", user.Email);
            }
        }
    }
}

public record DigestJobItem(string JobNumber, string Title, DateTime? DueDate);
