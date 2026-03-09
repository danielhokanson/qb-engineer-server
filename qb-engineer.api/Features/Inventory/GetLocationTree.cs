using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetLocationTreeQuery : IRequest<List<StorageLocationResponseModel>>;

public class GetLocationTreeHandler(IInventoryRepository repo) : IRequestHandler<GetLocationTreeQuery, List<StorageLocationResponseModel>>
{
    public Task<List<StorageLocationResponseModel>> Handle(GetLocationTreeQuery request, CancellationToken cancellationToken)
        => repo.GetLocationTreeAsync(cancellationToken);
}
