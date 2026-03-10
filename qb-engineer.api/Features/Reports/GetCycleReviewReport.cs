using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetCycleReviewReportQuery() : IRequest<List<CycleReviewReportItem>>;

public class GetCycleReviewReportHandler(IReportRepository repo) : IRequestHandler<GetCycleReviewReportQuery, List<CycleReviewReportItem>>
{
    public Task<List<CycleReviewReportItem>> Handle(GetCycleReviewReportQuery request, CancellationToken cancellationToken)
    {
        return repo.GetCycleReviewAsync(cancellationToken);
    }
}
