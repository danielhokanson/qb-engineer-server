using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Assets;

public record GetAssetsQuery(AssetType? Type, AssetStatus? Status, string? Search) : IRequest<List<AssetResponseModel>>;

public class GetAssetsHandler(IAssetRepository repo) : IRequestHandler<GetAssetsQuery, List<AssetResponseModel>>
{
    public Task<List<AssetResponseModel>> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
        => repo.GetAssetsAsync(request.Type, request.Status, request.Search, cancellationToken);
}
