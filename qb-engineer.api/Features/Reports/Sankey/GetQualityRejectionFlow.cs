using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetQualityRejectionFlowQuery(DateTimeOffset? Start, DateTimeOffset? End) : IRequest<List<SankeyFlowItem>>;

public class GetQualityRejectionFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetQualityRejectionFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetQualityRejectionFlowQuery request, CancellationToken cancellationToken)
        => repo.GetQualityRejectionFlowAsync(request.Start, request.End, cancellationToken);
}
