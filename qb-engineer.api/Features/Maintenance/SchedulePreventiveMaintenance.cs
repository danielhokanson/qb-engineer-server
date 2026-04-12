using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Maintenance;

public record SchedulePreventiveMaintenanceCommand(int Id) : IRequest;

public class SchedulePreventiveMaintenanceHandler(
    AppDbContext db,
    IPredictiveMaintenanceService predMaintService)
    : IRequestHandler<SchedulePreventiveMaintenanceCommand>
{
    public async Task Handle(SchedulePreventiveMaintenanceCommand command, CancellationToken cancellationToken)
    {
        var prediction = await db.MaintenancePredictions
            .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Prediction {command.Id} not found");

        if (prediction.Status == MaintenancePredictionStatus.MaintenanceScheduled)
            throw new InvalidOperationException("Maintenance already scheduled for this prediction");

        await predMaintService.ScheduleMaintenanceAsync(command.Id, cancellationToken);

        prediction.Status = MaintenancePredictionStatus.MaintenanceScheduled;
        await db.SaveChangesAsync(cancellationToken);
    }
}
