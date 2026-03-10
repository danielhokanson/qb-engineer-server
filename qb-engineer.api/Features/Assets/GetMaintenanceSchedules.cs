using MediatR;
using Microsoft.EntityFrameworkCore;

using QBEngineer.Core.Models;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Assets;

public record GetMaintenanceSchedulesQuery(int? AssetId) : IRequest<List<MaintenanceScheduleResponseModel>>;

public class GetMaintenanceSchedulesHandler(AppDbContext db)
    : IRequestHandler<GetMaintenanceSchedulesQuery, List<MaintenanceScheduleResponseModel>>
{
    public async Task<List<MaintenanceScheduleResponseModel>> Handle(
        GetMaintenanceSchedulesQuery request, CancellationToken ct)
    {
        var query = db.MaintenanceSchedules
            .Include(s => s.Asset)
            .Where(s => s.IsActive);

        if (request.AssetId.HasValue)
            query = query.Where(s => s.AssetId == request.AssetId.Value);

        var now = DateTime.UtcNow;

        return await query
            .OrderBy(s => s.NextDueAt)
            .Select(s => new MaintenanceScheduleResponseModel(
                s.Id,
                s.AssetId,
                s.Asset.Name,
                s.Title,
                s.Description,
                s.IntervalDays,
                s.IntervalHours,
                s.LastPerformedAt,
                s.NextDueAt,
                s.IsActive,
                s.NextDueAt < now))
            .ToListAsync(ct);
    }
}
