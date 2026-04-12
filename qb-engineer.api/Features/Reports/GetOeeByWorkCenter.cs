using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetOeeByWorkCenterQuery(int WorkCenterId, DateOnly From, DateOnly To) : IRequest<OeeCalculationModel>;

public class GetOeeByWorkCenterHandler(IOeeService oeeService)
    : IRequestHandler<GetOeeByWorkCenterQuery, OeeCalculationModel>
{
    public async Task<OeeCalculationModel> Handle(GetOeeByWorkCenterQuery request, CancellationToken ct)
    {
        return await oeeService.CalculateOeeAsync(request.WorkCenterId, request.From, request.To, ct);
    }
}
