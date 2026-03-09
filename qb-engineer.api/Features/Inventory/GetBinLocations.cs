using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetBinLocationsQuery : IRequest<List<StorageLocationFlatResponseModel>>;

public class GetBinLocationsHandler(IInventoryRepository repo) : IRequestHandler<GetBinLocationsQuery, List<StorageLocationFlatResponseModel>>
{
    public Task<List<StorageLocationFlatResponseModel>> Handle(GetBinLocationsQuery request, CancellationToken cancellationToken)
        => repo.GetBinLocationsAsync(cancellationToken);
}
