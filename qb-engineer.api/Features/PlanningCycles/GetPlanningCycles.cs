using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PlanningCycles;

public record GetPlanningCyclesQuery : IRequest<List<PlanningCycleListItemModel>>;

public class GetPlanningCyclesHandler(IPlanningCycleRepository repo)
    : IRequestHandler<GetPlanningCyclesQuery, List<PlanningCycleListItemModel>>
{
    public async Task<List<PlanningCycleListItemModel>> Handle(GetPlanningCyclesQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(cancellationToken);
    }
}
