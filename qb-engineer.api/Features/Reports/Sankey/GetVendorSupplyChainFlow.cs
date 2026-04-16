using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetVendorSupplyChainFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetVendorSupplyChainFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetVendorSupplyChainFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetVendorSupplyChainFlowQuery request, CancellationToken cancellationToken)
        => repo.GetVendorSupplyChainFlowAsync(cancellationToken);
}
