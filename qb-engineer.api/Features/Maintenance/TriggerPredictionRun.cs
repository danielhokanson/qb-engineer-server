using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Maintenance;

public record TriggerPredictionRunCommand(int WorkCenterId) : IRequest;

public class TriggerPredictionRunHandler(IPredictiveMaintenanceService predMaintService)
    : IRequestHandler<TriggerPredictionRunCommand>
{
    public async Task Handle(TriggerPredictionRunCommand command, CancellationToken cancellationToken)
    {
        await predMaintService.TriggerPredictionRunAsync(command.WorkCenterId, cancellationToken);
    }
}
