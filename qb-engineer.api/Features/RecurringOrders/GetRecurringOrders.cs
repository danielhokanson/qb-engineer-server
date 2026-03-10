using MediatR;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.RecurringOrders;

public record GetRecurringOrdersQuery(int? CustomerId, bool? IsActive) : IRequest<List<RecurringOrderListItemModel>>;

public class GetRecurringOrdersHandler(IRecurringOrderRepository repo)
    : IRequestHandler<GetRecurringOrdersQuery, List<RecurringOrderListItemModel>>
{
    public async Task<List<RecurringOrderListItemModel>> Handle(GetRecurringOrdersQuery request, CancellationToken cancellationToken)
    {
        return await repo.GetAllAsync(request.CustomerId, request.IsActive, cancellationToken);
    }
}
