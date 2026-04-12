using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Mrp;

public record GetMrpPeggingQuery(int MrpRunId, int PartId) : IRequest<List<MrpPeggingResponseModel>>;

public class GetMrpPeggingHandler(IMrpService mrpService)
    : IRequestHandler<GetMrpPeggingQuery, List<MrpPeggingResponseModel>>
{
    public async Task<List<MrpPeggingResponseModel>> Handle(GetMrpPeggingQuery request, CancellationToken cancellationToken)
    {
        return await mrpService.GetPeggingAsync(request.MrpRunId, request.PartId, cancellationToken);
    }
}
