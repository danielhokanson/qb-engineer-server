using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetQuoteToCashFlowQuery(DateTimeOffset? Start, DateTimeOffset? End) : IRequest<List<SankeyFlowItem>>;

public class GetQuoteToCashFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetQuoteToCashFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetQuoteToCashFlowQuery request, CancellationToken cancellationToken)
        => repo.GetQuoteToCashFlowAsync(request.Start, request.End, cancellationToken);
}
