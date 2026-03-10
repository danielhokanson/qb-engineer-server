using MediatR;

using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetCycleCountsQuery(int? LocationId, string? Status) : IRequest<List<CycleCountResponseModel>>;

public class GetCycleCountsHandler(IInventoryRepository repo)
    : IRequestHandler<GetCycleCountsQuery, List<CycleCountResponseModel>>
{
    public Task<List<CycleCountResponseModel>> Handle(
        GetCycleCountsQuery request, CancellationToken cancellationToken)
        => repo.GetCycleCountsAsync(request.LocationId, request.Status, cancellationToken);
}
