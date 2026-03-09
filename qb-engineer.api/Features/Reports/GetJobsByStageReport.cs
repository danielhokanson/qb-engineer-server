using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetJobsByStageReportQuery(int? TrackTypeId) : IRequest<List<JobsByStageReportItem>>;

public class GetJobsByStageReportHandler(IReportRepository repo) : IRequestHandler<GetJobsByStageReportQuery, List<JobsByStageReportItem>>
{
    public Task<List<JobsByStageReportItem>> Handle(GetJobsByStageReportQuery request, CancellationToken cancellationToken)
    {
        return repo.GetJobsByStageAsync(request.TrackTypeId, cancellationToken);
    }
}
