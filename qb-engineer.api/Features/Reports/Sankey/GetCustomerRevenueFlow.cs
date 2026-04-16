using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetCustomerRevenueFlowQuery(DateTimeOffset? Start, DateTimeOffset? End) : IRequest<List<SankeyFlowItem>>;

public class GetCustomerRevenueFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetCustomerRevenueFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetCustomerRevenueFlowQuery request, CancellationToken cancellationToken)
        => repo.GetCustomerRevenueFlowAsync(request.Start, request.End, cancellationToken);
}
