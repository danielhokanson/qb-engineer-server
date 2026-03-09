using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record CreateStorageLocationCommand(CreateStorageLocationRequestModel Data) : IRequest<StorageLocationResponseModel>;

public class CreateStorageLocationHandler(IInventoryRepository repo) : IRequestHandler<CreateStorageLocationCommand, StorageLocationResponseModel>
{
    public async Task<StorageLocationResponseModel> Handle(CreateStorageLocationCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;

        if (!string.IsNullOrWhiteSpace(data.Barcode) && await repo.BarcodeExistsAsync(data.Barcode, null, cancellationToken))
            throw new InvalidOperationException($"Barcode '{data.Barcode}' already exists.");

        if (data.ParentId.HasValue)
        {
            var parent = await repo.FindLocationAsync(data.ParentId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Parent location not found.");
        }

        var location = new StorageLocation
        {
            Name = data.Name.Trim(),
            LocationType = data.LocationType,
            ParentId = data.ParentId,
            Barcode = data.Barcode?.Trim(),
            Description = data.Description?.Trim(),
        };

        await repo.AddLocationAsync(location, cancellationToken);

        return new StorageLocationResponseModel(
            location.Id, location.Name, location.LocationType, location.ParentId,
            location.Barcode, location.Description, location.SortOrder, location.IsActive,
            location.Name, 0, []);
    }
}
