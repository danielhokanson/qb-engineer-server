using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobCostSummaryQuery(int JobId) : IRequest<JobCostSummaryModel>;

public class GetJobCostSummaryHandler(IJobCostService costService)
    : IRequestHandler<GetJobCostSummaryQuery, JobCostSummaryModel>
{
    public Task<JobCostSummaryModel> Handle(GetJobCostSummaryQuery request, CancellationToken cancellationToken)
    {
        return costService.GetCostSummaryAsync(request.JobId, cancellationToken);
    }
}
