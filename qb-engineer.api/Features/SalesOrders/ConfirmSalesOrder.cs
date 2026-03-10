using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.SalesOrders;

public record ConfirmSalesOrderCommand(int Id) : IRequest;

public class ConfirmSalesOrderHandler(ISalesOrderRepository repo)
    : IRequestHandler<ConfirmSalesOrderCommand>
{
    public async Task Handle(ConfirmSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order {request.Id} not found");

        if (order.Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft orders can be confirmed");

        order.Status = SalesOrderStatus.Confirmed;
        order.ConfirmedDate = DateTime.UtcNow;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
