using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.SalesOrders;

public record CancelSalesOrderCommand(int Id) : IRequest;

public class CancelSalesOrderHandler(ISalesOrderRepository repo)
    : IRequestHandler<CancelSalesOrderCommand>
{
    public async Task Handle(CancelSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order {request.Id} not found");

        if (order.Status == SalesOrderStatus.Shipped || order.Status == SalesOrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel shipped or completed orders");

        order.Status = SalesOrderStatus.Cancelled;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
