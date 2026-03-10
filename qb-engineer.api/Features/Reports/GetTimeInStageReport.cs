using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetTimeInStageReportQuery(int? TrackTypeId) : IRequest<List<TimeInStageReportItem>>;

public class GetTimeInStageHandler(IReportRepository repo)
    : IRequestHandler<GetTimeInStageReportQuery, List<TimeInStageReportItem>>
{
    public async Task<List<TimeInStageReportItem>> Handle(GetTimeInStageReportQuery request, CancellationToken ct)
    {
        return await repo.GetTimeInStageAsync(request.TrackTypeId, ct);
    }
}
