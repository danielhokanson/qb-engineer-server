using System.Security.Claims;

using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Core.Enums;
using QBEngineer.Data.Context;

namespace QBEngineer.Api.Features.Inventory;

public record WaiveInspectionCommand(int ReceivingRecordId) : IRequest;

public class WaiveInspectionHandler(AppDbContext db, IHttpContextAccessor httpContext)
    : IRequestHandler<WaiveInspectionCommand>
{
    public async Task Handle(WaiveInspectionCommand request, CancellationToken ct)
    {
        var record = await db.ReceivingRecords.FindAsync([request.ReceivingRecordId], ct)
            ?? throw new KeyNotFoundException($"ReceivingRecord {request.ReceivingRecordId} not found.");

        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        record.InspectionStatus = ReceivingInspectionStatus.Waived;
        record.InspectedById = userId;
        record.InspectedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
