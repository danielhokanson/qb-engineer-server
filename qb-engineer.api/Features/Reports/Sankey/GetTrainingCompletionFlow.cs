using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports.Sankey;

public record GetTrainingCompletionFlowQuery : IRequest<List<SankeyFlowItem>>;

public class GetTrainingCompletionFlowHandler(ISankeyReportRepository repo)
    : IRequestHandler<GetTrainingCompletionFlowQuery, List<SankeyFlowItem>>
{
    public Task<List<SankeyFlowItem>> Handle(GetTrainingCompletionFlowQuery request, CancellationToken cancellationToken)
        => repo.GetTrainingCompletionFlowAsync(cancellationToken);
}
