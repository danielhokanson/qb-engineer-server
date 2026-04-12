using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetOeeReportQuery(DateOnly From, DateOnly To) : IRequest<IReadOnlyList<OeeCalculationModel>>;

public class GetOeeReportHandler(IOeeService oeeService)
    : IRequestHandler<GetOeeReportQuery, IReadOnlyList<OeeCalculationModel>>
{
    public async Task<IReadOnlyList<OeeCalculationModel>> Handle(GetOeeReportQuery request, CancellationToken ct)
    {
        return await oeeService.CalculateOeeForAllWorkCentersAsync(request.From, request.To, ct);
    }
}
