using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Reports;

public record GetSixBigLossesQuery(int WorkCenterId, DateOnly From, DateOnly To) : IRequest<SixBigLossesModel>;

public class GetSixBigLossesHandler(IOeeService oeeService)
    : IRequestHandler<GetSixBigLossesQuery, SixBigLossesModel>
{
    public async Task<SixBigLossesModel> Handle(GetSixBigLossesQuery request, CancellationToken ct)
    {
        return await oeeService.GetSixBigLossesAsync(request.WorkCenterId, request.From, request.To, ct);
    }
}
