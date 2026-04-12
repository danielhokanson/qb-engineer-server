using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.TimeTracking;

public record GetOperationTimeEntriesQuery(int JobId, int OperationId) : IRequest<List<TimeEntryResponseModel>>;

public class GetOperationTimeEntriesHandler(AppDbContext db)
    : IRequestHandler<GetOperationTimeEntriesQuery, List<TimeEntryResponseModel>>
{
    public async Task<List<TimeEntryResponseModel>> Handle(
        GetOperationTimeEntriesQuery request, CancellationToken cancellationToken)
    {
        return await db.TimeEntries
            .AsNoTracking()
            .Where(t => t.JobId == request.JobId && t.OperationId == request.OperationId)
            .OrderByDescending(t => t.Date)
            .Select(t => new TimeEntryResponseModel
            {
                Id = t.Id,
                JobId = t.JobId,
                UserId = t.UserId,
                Date = t.Date,
                DurationMinutes = t.DurationMinutes,
                Category = t.Category,
                Notes = t.Notes,
                TimerStart = t.TimerStart,
                TimerStop = t.TimerStop,
                IsManual = t.IsManual,
                IsLocked = t.IsLocked,
                LaborCost = t.LaborCost,
                BurdenCost = t.BurdenCost,
                OperationId = t.OperationId,
                EntryType = t.EntryType.ToString(),
            })
            .ToListAsync(cancellationToken);
    }
}
