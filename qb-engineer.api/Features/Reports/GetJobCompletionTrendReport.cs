using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetJobCompletionTrendQuery(int Months = 6) : IRequest<List<JobCompletionTrendItem>>;

public class GetJobCompletionTrendHandler(IReportRepository repo) : IRequestHandler<GetJobCompletionTrendQuery, List<JobCompletionTrendItem>>
{
    public Task<List<JobCompletionTrendItem>> Handle(GetJobCompletionTrendQuery request, CancellationToken cancellationToken)
    {
        return repo.GetJobCompletionTrendAsync(request.Months, cancellationToken);
    }
}
