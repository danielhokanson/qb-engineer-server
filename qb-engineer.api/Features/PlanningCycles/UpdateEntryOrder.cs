using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PlanningCycles;

public record UpdateEntryOrderCommand(int CycleId, List<EntryOrderItem> Items) : IRequest;

public class UpdateEntryOrderHandler(IPlanningCycleRepository repo)
    : IRequestHandler<UpdateEntryOrderCommand>
{
    public async Task Handle(UpdateEntryOrderCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Items)
        {
            var entry = await repo.FindEntryAsync(request.CycleId, item.JobId, cancellationToken);
            if (entry != null)
                entry.SortOrder = item.SortOrder;
        }

        await repo.SaveChangesAsync(cancellationToken);
    }
}
