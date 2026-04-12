using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Maintenance;

public record GetPredictiveMaintenanceDashboardQuery : IRequest<PredictiveMaintenanceDashboardResponseModel>;

public class GetPredictiveMaintenanceDashboardHandler(IPredictiveMaintenanceService predMaintService)
    : IRequestHandler<GetPredictiveMaintenanceDashboardQuery, PredictiveMaintenanceDashboardResponseModel>
{
    public async Task<PredictiveMaintenanceDashboardResponseModel> Handle(
        GetPredictiveMaintenanceDashboardQuery request, CancellationToken cancellationToken)
    {
        return await predMaintService.GetDashboardAsync(cancellationToken);
    }
}
