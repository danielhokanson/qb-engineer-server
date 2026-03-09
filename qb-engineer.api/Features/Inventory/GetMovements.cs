using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetMovementsQuery(int? LocationId, string? EntityType, int? EntityId, int Take = 100) : IRequest<List<BinMovementResponseModel>>;

public class GetMovementsHandler(IInventoryRepository repo) : IRequestHandler<GetMovementsQuery, List<BinMovementResponseModel>>
{
    public Task<List<BinMovementResponseModel>> Handle(GetMovementsQuery request, CancellationToken cancellationToken)
        => repo.GetMovementsAsync(request.LocationId, request.EntityType, request.EntityId, request.Take, cancellationToken);
}
