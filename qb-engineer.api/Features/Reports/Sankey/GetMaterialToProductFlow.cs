using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetMaterialToProductFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetMaterialToProductFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetMaterialToProductFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetMaterialToProductFlowQuery request, CancellationToken cancellationToken)
        => repo.GetMaterialToProductFlowAsync(cancellationToken);
}
