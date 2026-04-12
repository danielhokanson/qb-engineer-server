using MediatR;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Mrp;

public record SimulateMrpRunCommand(
    MrpRunType RunType = MrpRunType.Simulation,
    int PlanningHorizonDays = 90,
    List<int>? PartIds = null,
    int? InitiatedByUserId = null
) : IRequest<MrpRunResponseModel>;

public class SimulateMrpRunHandler(IMrpService mrpService)
    : IRequestHandler<SimulateMrpRunCommand, MrpRunResponseModel>
{
    public async Task<MrpRunResponseModel> Handle(SimulateMrpRunCommand request, CancellationToken cancellationToken)
    {
        var options = new MrpRunOptions(
            RunType: request.RunType,
            PlanningHorizonDays: request.PlanningHorizonDays,
            PartIds: request.PartIds,
            IsSimulation: true,
            InitiatedByUserId: request.InitiatedByUserId
        );

        return await mrpService.ExecuteRunAsync(options, cancellationToken);
    }
}
