using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs.Links;

public record GetJobLinksQuery(int JobId) : IRequest<List<JobLinkResponseModel>>;

public class GetJobLinksHandler(IJobLinkRepository repo) : IRequestHandler<GetJobLinksQuery, List<JobLinkResponseModel>>
{
    public Task<List<JobLinkResponseModel>> Handle(GetJobLinksQuery request, CancellationToken cancellationToken)
        => repo.GetByJobIdAsync(request.JobId, cancellationToken);
}
