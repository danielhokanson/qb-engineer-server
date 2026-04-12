using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Scheduling;

public record GetWorkCenterLoadQuery(int WorkCenterId, DateOnly From, DateOnly To) : IRequest<WorkCenterLoadResponseModel>;

public class GetWorkCenterLoadHandler(ISchedulingService schedulingService) : IRequestHandler<GetWorkCenterLoadQuery, WorkCenterLoadResponseModel>
{
    public async Task<WorkCenterLoadResponseModel> Handle(GetWorkCenterLoadQuery request, CancellationToken cancellationToken)
    {
        return await schedulingService.GetWorkCenterLoadAsync(request.WorkCenterId, request.From, request.To, cancellationToken);
    }
}
