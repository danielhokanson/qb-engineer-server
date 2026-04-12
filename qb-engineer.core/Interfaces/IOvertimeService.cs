using QBEngineer.Core.Models;

namespace QBEngineer.Core.Interfaces;

public interface IOvertimeService
{
    Task<OvertimeBreakdownResponseModel> CalculateOvertimeAsync(int userId, DateOnly weekStart, DateOnly weekEnd, CancellationToken ct);
}
