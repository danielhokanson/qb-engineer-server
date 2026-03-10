using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetOnTimeDeliveryReportQuery(DateTimeOffset Start, DateTimeOffset End) : IRequest<OnTimeDeliveryReportItem>;

public class GetOnTimeDeliveryReportHandler(IReportRepository repo)
    : IRequestHandler<GetOnTimeDeliveryReportQuery, OnTimeDeliveryReportItem>
{
    public async Task<OnTimeDeliveryReportItem> Handle(
        GetOnTimeDeliveryReportQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetOnTimeDeliveryAsync(request.Start, request.End, cancellationToken);
    }
}
