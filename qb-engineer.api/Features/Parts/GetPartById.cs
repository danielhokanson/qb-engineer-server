using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record GetPartByIdQuery(int Id) : IRequest<PartDetailResponseModel>;

public class GetPartByIdHandler(IPartRepository repo) : IRequestHandler<GetPartByIdQuery, PartDetailResponseModel>
{
    public async Task<PartDetailResponseModel> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await repo.GetDetailAsync(request.Id, cancellationToken);
        return part ?? throw new KeyNotFoundException($"Part {request.Id} not found");
    }
}
