using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobsQuery(
    int? TrackTypeId,
    int? CurrentStageId,
    int? AssigneeId,
    bool IsArchived = false,
    string? Search = null) : IRequest<List<JobListResponseModel>>;

public class GetJobsHandler(IJobRepository repo) : IRequestHandler<GetJobsQuery, List<JobListResponseModel>>
{
    public Task<List<JobListResponseModel>> Handle(GetJobsQuery request, CancellationToken cancellationToken)
    {
        return repo.GetJobsAsync(
            request.TrackTypeId,
            request.CurrentStageId,
            request.AssigneeId,
            request.IsArchived,
            request.Search,
            cancellationToken);
    }
}
