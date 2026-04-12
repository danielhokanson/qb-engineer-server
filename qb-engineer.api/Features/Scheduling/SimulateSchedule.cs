using MediatR;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Scheduling;

public record SimulateScheduleCommand(
    ScheduleDirection Direction,
    DateOnly ScheduleFrom,
    DateOnly ScheduleTo,
    int[]? JobIdFilter,
    string PriorityRule,
    int? RunByUserId) : IRequest<ScheduleRunResponseModel>;

public class SimulateScheduleHandler(ISchedulingService schedulingService) : IRequestHandler<SimulateScheduleCommand, ScheduleRunResponseModel>
{
    public async Task<ScheduleRunResponseModel> Handle(SimulateScheduleCommand request, CancellationToken cancellationToken)
    {
        var parameters = new ScheduleParameters(
            request.Direction, request.ScheduleFrom, request.ScheduleTo,
            request.JobIdFilter, request.PriorityRule, request.RunByUserId);

        return await schedulingService.SimulateAsync(parameters, cancellationToken);
    }
}
