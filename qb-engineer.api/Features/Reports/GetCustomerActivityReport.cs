using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetCustomerActivityReportQuery : IRequest<List<CustomerActivityReportItem>>;

public class GetCustomerActivityReportHandler(IReportRepository repo)
    : IRequestHandler<GetCustomerActivityReportQuery, List<CustomerActivityReportItem>>
{
    public async Task<List<CustomerActivityReportItem>> Handle(
        GetCustomerActivityReportQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetCustomerActivityAsync(cancellationToken);
    }
}
