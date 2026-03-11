using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.StatusTracking;

public record GetActiveStatusQuery(string EntityType, int EntityId) : IRequest<ActiveStatusResponseModel>;

public class GetActiveStatusHandler(IStatusEntryRepository repository)
    : IRequestHandler<GetActiveStatusQuery, ActiveStatusResponseModel>
{
    public async Task<ActiveStatusResponseModel> Handle(
        GetActiveStatusQuery request, CancellationToken cancellationToken)
    {
        var workflowStatus = await repository.GetCurrentWorkflowStatusAsync(
            request.EntityType, request.EntityId, cancellationToken);

        var activeHolds = await repository.GetActiveHoldsAsync(
            request.EntityType, request.EntityId, cancellationToken);

        return new ActiveStatusResponseModel(workflowStatus, activeHolds);
    }
}
