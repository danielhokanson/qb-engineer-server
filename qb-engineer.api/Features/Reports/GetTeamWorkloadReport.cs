using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetTeamWorkloadReportQuery : IRequest<List<TeamWorkloadReportItem>>;

public class GetTeamWorkloadReportHandler(IReportRepository repo)
    : IRequestHandler<GetTeamWorkloadReportQuery, List<TeamWorkloadReportItem>>
{
    public async Task<List<TeamWorkloadReportItem>> Handle(
        GetTeamWorkloadReportQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetTeamWorkloadAsync(cancellationToken);
    }
}
