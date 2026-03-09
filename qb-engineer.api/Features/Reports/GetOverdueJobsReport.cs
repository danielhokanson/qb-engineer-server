using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetOverdueJobsReportQuery : IRequest<List<OverdueJobReportItem>>;

public class GetOverdueJobsReportHandler(IReportRepository repo) : IRequestHandler<GetOverdueJobsReportQuery, List<OverdueJobReportItem>>
{
    public Task<List<OverdueJobReportItem>> Handle(GetOverdueJobsReportQuery request, CancellationToken cancellationToken)
    {
        return repo.GetOverdueJobsAsync(cancellationToken);
    }
}
