using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetExpenseFlowQuery(DateTimeOffset? Start, DateTimeOffset? End) : IRequest<List<SankeyFlowItem>>;

public class GetExpenseFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetExpenseFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetExpenseFlowQuery request, CancellationToken cancellationToken)
        => repo.GetExpenseFlowAsync(request.Start, request.End, cancellationToken);
}
