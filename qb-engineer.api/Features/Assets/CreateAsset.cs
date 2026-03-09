using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Assets;

public record CreateAssetCommand(CreateAssetRequestModel Data) : IRequest<AssetResponseModel>;

public class CreateAssetHandler(IAssetRepository repo) : IRequestHandler<CreateAssetCommand, AssetResponseModel>
{
    public async Task<AssetResponseModel> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var asset = new Asset
        {
            Name = data.Name.Trim(),
            AssetType = data.AssetType,
            Location = data.Location?.Trim(),
            Manufacturer = data.Manufacturer?.Trim(),
            Model = data.Model?.Trim(),
            SerialNumber = data.SerialNumber?.Trim(),
            Notes = data.Notes?.Trim(),
        };

        await repo.AddAsync(asset, cancellationToken);
        return (await repo.GetByIdAsync(asset.Id, cancellationToken))!;
    }
}
