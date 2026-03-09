using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Parts;

public record GetPartsQuery(PartStatus? Status, PartType? Type, string? Search) : IRequest<List<PartListResponseModel>>;

public class GetPartsHandler(IPartRepository repo) : IRequestHandler<GetPartsQuery, List<PartListResponseModel>>
{
    public Task<List<PartListResponseModel>> Handle(GetPartsQuery request, CancellationToken cancellationToken)
        => repo.GetPartsAsync(request.Status, request.Type, request.Search, cancellationToken);
}
