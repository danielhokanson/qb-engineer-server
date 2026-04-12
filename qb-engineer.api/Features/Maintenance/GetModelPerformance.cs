using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Maintenance;

public record GetModelPerformanceQuery(string ModelId) : IRequest<MlModelPerformanceResponseModel>;

public class GetModelPerformanceHandler(IPredictiveMaintenanceService predMaintService)
    : IRequestHandler<GetModelPerformanceQuery, MlModelPerformanceResponseModel>
{
    public async Task<MlModelPerformanceResponseModel> Handle(
        GetModelPerformanceQuery request, CancellationToken cancellationToken)
    {
        return await predMaintService.GetModelPerformanceAsync(request.ModelId, cancellationToken);
    }
}
