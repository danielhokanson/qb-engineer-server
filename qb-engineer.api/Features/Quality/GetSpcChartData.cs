using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Quality;

public record GetSpcChartDataQuery(int CharacteristicId, int? LastNSubgroups) : IRequest<SpcChartDataModel>;

public class GetSpcChartDataHandler(ISpcService spcService)
    : IRequestHandler<GetSpcChartDataQuery, SpcChartDataModel>
{
    public async Task<SpcChartDataModel> Handle(
        GetSpcChartDataQuery request, CancellationToken cancellationToken)
    {
        return await spcService.GetXBarRChartDataAsync(
            request.CharacteristicId, request.LastNSubgroups, cancellationToken);
    }
}
