using MediatR;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record SubmitPurchaseOrderCommand(int Id) : IRequest;

public class SubmitPurchaseOrderHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<SubmitPurchaseOrderCommand>
{
    public async Task Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.Id} not found");

        if (po.Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft purchase orders can be submitted");

        po.Status = PurchaseOrderStatus.Submitted;
        po.SubmittedDate = DateTime.UtcNow;

        await repo.SaveChangesAsync(cancellationToken);
    }
}
