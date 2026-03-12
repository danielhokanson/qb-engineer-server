using FluentValidation;
using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Assets;

public record CreateAssetCommand(CreateAssetRequestModel Data) : IRequest<AssetResponseModel>;

public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.Data.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.SerialNumber).MaximumLength(100).When(x => x.Data.SerialNumber is not null);
        RuleFor(x => x.Data.Location).MaximumLength(200).When(x => x.Data.Location is not null);
    }
}

public class CreateAssetHandler(IAssetRepository repo, IBarcodeService barcodeService) : IRequestHandler<CreateAssetCommand, AssetResponseModel>
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
            IsCustomerOwned = data.IsCustomerOwned ?? false,
            CavityCount = data.CavityCount,
            ToolLifeExpectancy = data.ToolLifeExpectancy,
            SourceJobId = data.SourceJobId,
            SourcePartId = data.SourcePartId,
        };

        await repo.AddAsync(asset, cancellationToken);

        var naturalId = asset.SerialNumber ?? asset.Name;
        await barcodeService.CreateBarcodeAsync(
            BarcodeEntityType.Asset, asset.Id, naturalId, cancellationToken);

        return (await repo.GetByIdAsync(asset.Id, cancellationToken))!;
    }
}
