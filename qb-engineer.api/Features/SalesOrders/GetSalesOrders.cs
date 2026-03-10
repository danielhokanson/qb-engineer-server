using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.SalesOrders;

public record GetSalesOrdersQuery(int? CustomerId, SalesOrderStatus? Status) : IRequest<List<SalesOrderListItemModel>>;

public class GetSalesOrdersHandler(ISalesOrderRepository repo)
    : IRequestHandler<GetSalesOrdersQuery, List<SalesOrderListItemModel>>
{
    public async Task<List<SalesOrderListItemModel>> Handle(GetSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.CustomerId, request.Status, cancellationToken);
    }
}
