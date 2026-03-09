using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Jobs;

public record GetJobByIdQuery(int Id) : IRequest<JobDetailResponseModel>;

public class GetJobByIdHandler(IJobRepository repo) : IRequestHandler<GetJobByIdQuery, JobDetailResponseModel>
{
    public async Task<JobDetailResponseModel> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetDetailAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job with ID {request.Id} not found.");
    }
}
