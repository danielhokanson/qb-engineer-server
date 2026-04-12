using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface ICopqService
{
    Task<CopqReportResponseModel> GenerateReportAsync(DateOnly periodStart, DateOnly periodEnd, CancellationToken ct);
    Task<IReadOnlyList<CopqTrendPointResponseModel>> GetTrendAsync(int months, CancellationToken ct);
    Task<IReadOnlyList<CopqParetoItemResponseModel>> GetParetoByDefectAsync(DateOnly periodStart, DateOnly periodEnd, CancellationToken ct);
}
