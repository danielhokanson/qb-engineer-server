using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetJobStageFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetJobStageFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetJobStageFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetJobStageFlowQuery request, CancellationToken cancellationToken)
        => repo.GetJobStageFlowAsync(cancellationToken);
}
