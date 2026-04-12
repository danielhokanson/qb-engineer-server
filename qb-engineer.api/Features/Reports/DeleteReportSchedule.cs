using MediatR;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Reports;

public record DeleteReportScheduleCommand(int Id) : IRequest;

public class DeleteReportScheduleHandler(AppDbContext db) : IRequestHandler<DeleteReportScheduleCommand>
{
    public async Task Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await db.ReportSchedules.FindAsync([request.Id], cancellationToken)
            ?? throw new KeyNotFoundException($"Report schedule {request.Id} not found.");

        db.ReportSchedules.Remove(schedule);
        await db.SaveChangesAsync(cancellationToken);
    }
}
