using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IReportRepository
{
    Task<List<JobsByStageReportItem>> GetJobsByStageAsync(int? trackTypeId, CancellationToken ct);
    Task<List<OverdueJobReportItem>> GetOverdueJobsAsync(CancellationToken ct);
    Task<List<TimeByUserReportItem>> GetTimeByUserAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct);
    Task<List<ExpenseSummaryReportItem>> GetExpenseSummaryAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken ct);
    Task<List<LeadPipelineReportItem>> GetLeadPipelineAsync(CancellationToken ct);
    Task<List<JobCompletionTrendItem>> GetJobCompletionTrendAsync(int months, CancellationToken ct);
}
