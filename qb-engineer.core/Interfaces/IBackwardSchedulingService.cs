using QBEngineer.Core.Entities;

namespace QBEngineer.Core.Interfaces;

public interface IBackwardSchedulingService
{
    Task<List<ScheduleMilestone>> CalculateMilestonesAsync(int salesOrderLineId, CancellationToken ct = default);
}
