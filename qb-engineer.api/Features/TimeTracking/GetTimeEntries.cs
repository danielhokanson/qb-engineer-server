using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.TimeTracking;

public record GetTimeEntriesQuery(int? UserId, int? JobId, DateOnly? From, DateOnly? To) : IRequest<List<TimeEntryResponseModel>>;

public class GetTimeEntriesHandler(ITimeTrackingRepository repo) : IRequestHandler<GetTimeEntriesQuery, List<TimeEntryResponseModel>>
{
    public Task<List<TimeEntryResponseModel>> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
        => repo.GetTimeEntriesAsync(request.UserId, request.JobId, request.From, request.To, cancellationToken);
}
