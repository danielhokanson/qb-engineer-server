using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetInventoryLocationFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetInventoryLocationFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetInventoryLocationFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetInventoryLocationFlowQuery request, CancellationToken cancellationToken)
        => repo.GetInventoryLocationFlowAsync(cancellationToken);
}
