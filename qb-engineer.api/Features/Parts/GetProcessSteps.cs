using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record GetProcessStepsQuery(int PartId) : IRequest<List<ProcessStepResponseModel>>;

public class GetProcessStepsHandler(IPartRepository repo) : IRequestHandler<GetProcessStepsQuery, List<ProcessStepResponseModel>>
{
    public async Task<List<ProcessStepResponseModel>> Handle(GetProcessStepsQuery request, CancellationToken cancellationToken)
    {
        _ = await repo.FindAsync(request.PartId, cancellationToken)
            ?? throw new KeyNotFoundException($"Part {request.PartId} not found");

        return await repo.GetProcessStepsAsync(request.PartId, cancellationToken);
    }
}
