using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record GetPurchaseOrdersQuery(int? VendorId, int? JobId, PurchaseOrderStatus? Status) : IRequest<List<PurchaseOrderListItemModel>>;

public class GetPurchaseOrdersHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<GetPurchaseOrdersQuery, List<PurchaseOrderListItemModel>>
{
    public async Task<List<PurchaseOrderListItemModel>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.VendorId, request.JobId, request.Status, cancellationToken);
    }
}
