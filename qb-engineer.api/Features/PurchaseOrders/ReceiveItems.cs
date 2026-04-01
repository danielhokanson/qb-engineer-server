using MediatR;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record ReceiveItemsCommand(int PurchaseOrderId, List<ReceiveLineModel> Lines) : IRequest;

public class ReceiveItemsHandler(IPurchaseOrderRepository repo)
    : IRequestHandler<ReceiveItemsCommand>
{
    public async Task Handle(ReceiveItemsCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindWithDetailsAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.PurchaseOrderId} not found");

        if (po.Status == PurchaseOrderStatus.Closed || po.Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot receive items on a closed or cancelled purchase order");

        foreach (var receiveItem in request.Lines)
        {
            var line = po.Lines.FirstOrDefault(l => l.Id == receiveItem.LineId)
                ?? throw new KeyNotFoundException($"Line {receiveItem.LineId} not found on this purchase order");

            if (receiveItem.Quantity <= 0)
                throw new InvalidOperationException("Receive quantity must be positive");

            if (receiveItem.Quantity > line.RemainingQuantity)
                throw new InvalidOperationException($"Cannot receive {receiveItem.Quantity} — only {line.RemainingQuantity} remaining");

            line.ReceivedQuantity += receiveItem.Quantity;

            await repo.AddReceivingRecordAsync(new ReceivingRecord
            {
                PurchaseOrderLineId = line.Id,
                QuantityReceived = receiveItem.Quantity,
                StorageLocationId = receiveItem.StorageLocationId,
                Notes = receiveItem.Notes,
            }, cancellationToken);
        }

        var allReceived = po.Lines.All(l => l.RemainingQuantity <= 0);
        var anyReceived = po.Lines.Any(l => l.ReceivedQuantity > 0);

        if (allReceived)
        {
            po.Status = PurchaseOrderStatus.Received;
            po.ReceivedDate = DateTimeOffset.UtcNow;
        }
        else if (anyReceived)
        {
            po.Status = PurchaseOrderStatus.PartiallyReceived;
        }

        await repo.SaveChangesAsync(cancellationToken);
    }
}
