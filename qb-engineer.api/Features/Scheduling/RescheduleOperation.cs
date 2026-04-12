using MediatR;

using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.Scheduling;

public record RescheduleOperationCommand(int ScheduledOperationId, DateTimeOffset NewStart) : IRequest;

public class RescheduleOperationHandler(ISchedulingService schedulingService) : IRequestHandler<RescheduleOperationCommand>
{
    public async Task Handle(RescheduleOperationCommand request, CancellationToken cancellationToken)
    {
        await schedulingService.RescheduleOperationAsync(request.ScheduledOperationId, request.NewStart, cancellationToken);
    }
}
