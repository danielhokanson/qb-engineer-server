using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TimeTracking;

public record UpdateTimeEntryCommand(int Id, UpdateTimeEntryRequestModel Data) : IRequest<TimeEntryResponseModel>;

public class UpdateTimeEntryHandler(ITimeTrackingRepository repo) : IRequestHandler<UpdateTimeEntryCommand, TimeEntryResponseModel>
{
    public async Task<TimeEntryResponseModel> Handle(UpdateTimeEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await repo.FindTimeEntryAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Time entry {request.Id} not found.");

        if (entry.IsLocked)
            throw new InvalidOperationException("This time entry is locked and cannot be edited.");

        var data = request.Data;
        if (data.JobId.HasValue) entry.JobId = data.JobId;
        if (data.Date.HasValue) entry.Date = data.Date.Value;
        if (data.DurationMinutes.HasValue) entry.DurationMinutes = data.DurationMinutes.Value;
        if (data.Category is not null) entry.Category = data.Category.Trim();
        if (data.Notes is not null) entry.Notes = data.Notes.Trim();

        await repo.SaveChangesAsync(cancellationToken);

        return (await repo.GetTimeEntryByIdAsync(entry.Id, cancellationToken))!;
    }
}
