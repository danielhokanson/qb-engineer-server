using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Mrp;

public record GetMrpPartPlanQuery(int MrpRunId, int PartId) : IRequest<MrpPartPlanResponseModel>;

public class GetMrpPartPlanHandler(IMrpService mrpService)
    : IRequestHandler<GetMrpPartPlanQuery, MrpPartPlanResponseModel>
{
    public async Task<MrpPartPlanResponseModel> Handle(GetMrpPartPlanQuery request, CancellationToken cancellationToken)
    {
        return await mrpService.GetPartPlanAsync(request.MrpRunId, request.PartId, cancellationToken);
    }
}
