using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetWorkerOrdersFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetWorkerOrdersFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetWorkerOrdersFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetWorkerOrdersFlowQuery request, CancellationToken cancellationToken)
        => repo.GetWorkerOrdersFlowAsync(cancellationToken);
}
