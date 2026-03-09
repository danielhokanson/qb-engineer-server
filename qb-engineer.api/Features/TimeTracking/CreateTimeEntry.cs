using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TimeTracking;

public record CreateTimeEntryCommand(CreateTimeEntryRequestModel Data) : IRequest<TimeEntryResponseModel>;

public class CreateTimeEntryHandler(ITimeTrackingRepository repo, IHttpContextAccessor httpContext) : IRequestHandler<CreateTimeEntryCommand, TimeEntryResponseModel>
{
    public async Task<TimeEntryResponseModel> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var userId = int.Parse(httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var entry = new TimeEntry
        {
            UserId = userId,
            JobId = data.JobId,
            Date = data.Date,
            DurationMinutes = data.DurationMinutes,
            Category = data.Category?.Trim(),
            Notes = data.Notes?.Trim(),
            IsManual = true,
        };

        await repo.AddTimeEntryAsync(entry, cancellationToken);

        return (await repo.GetTimeEntryByIdAsync(entry.Id, cancellationToken))!;
    }
}
