using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.Inventory;

public record GetPartInventoryQuery(string? Search) : IRequest<List<InventoryPartSummaryResponseModel>>;

public class GetPartInventoryHandler(IInventoryRepository repo) : IRequestHandler<GetPartInventoryQuery, List<InventoryPartSummaryResponseModel>>
{
    public Task<List<InventoryPartSummaryResponseModel>> Handle(GetPartInventoryQuery request, CancellationToken cancellationToken)
        => repo.GetPartInventorySummaryAsync(request.Search, cancellationToken);
}
