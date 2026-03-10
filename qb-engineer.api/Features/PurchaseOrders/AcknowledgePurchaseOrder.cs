using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record AcknowledgePurchaseOrderCommand(int Id, DateTime? ExpectedDeliveryDate) : IRequest;

public class AcknowledgePurchaseOrderHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<AcknowledgePurchaseOrderCommand>
{
    public async Task Handle(AcknowledgePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found");

        if (po.Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException("Only Submitted purchase orders can be acknowledged");

        po.Status = PurchaseOrderStatus.Acknowledged;
        po.AcknowledgedDate = DateTime.UtcNow;
        if (request.ExpectedDeliveryDate.HasValue)
            po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
