using MediatR;
using Microsoft.AspNetCore.Http;

using QBEngineer.Api.Features.DomainEvents;
using QBEngineer.Core.Entities;
using QBEngineer.Core.Enums;
using QBEngineer.Core.Interfaces;
using QBEngineer.Core.Models;

namespace QBEngineer.Api.Features.PurchaseOrders;

public record ReceiveItemsCommand(
    int PurchaseOrderId,
    List<ReceiveLineModel> Lines,
    decimal? ActualFreight = null,
    FreightAllocationMethod FreightAllocationMethod = FreightAllocationMethod.ByExtendedValue) : IRequest;

public class ReceiveItemsHandler(
    IPurchaseOrderRepository repo,
    IClock clock,
    IMediator mediator,
    IHttpContextAccessor httpContext)
    : IRequestHandler<ReceiveItemsCommand>
{
    public async Task Handle(ReceiveItemsCommand request, CancellationToken cancellationToken)
    {
        var po = await repo.FindWithDetailsAsync(request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order {request.PurchaseOrderId} not found");

        if (po.Status == PurchaseOrderStatus.Closed || po.Status == PurchaseOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot receive items on a closed or cancelled purchase order");

        // Bought-parts effort PR3 — receipt-level freight capture. All
        // ReceivingRecords created in this call share a ReceiptNumber and
        // the same ActualFreight value (each row gets a per-line slice in
        // AllocatedFreight via the chosen allocation method). When the
        // caller didn't supply ActualFreight, default it from the PO's
        // EstimatedFreight; the buyer "matches estimate" with one click
        // (the variance check is a UI concern; we record the captured
        // value either way).
        var receiptNumber = $"R-{clock.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        var actualFreight = request.ActualFreight ?? po.EstimatedFreight;

        // Pre-compute per-line allocation. ByExtendedValue / ByQuantity
        // need the totals, so we collect first then divvy up. Manual reads
        // each request line's ManualFreight directly. Weight allocation
        // falls back to ByExtendedValue when any part lacks a weight.
        var newRecords = new List<(ReceiveLineModel req, PurchaseOrderLine line, ReceivingRecord rec)>();
        decimal totalExtended = 0m;
        decimal totalQty = 0m;
        foreach (var receiveItem in request.Lines)
        {
            var line = po.Lines.FirstOrDefault(l => l.Id == receiveItem.LineId)
                ?? throw new KeyNotFoundException($"Line {receiveItem.LineId} not found on this purchase order");

            if (receiveItem.Quantity <= 0)
                throw new InvalidOperationException("Receive quantity must be positive");

            if (receiveItem.Quantity > line.RemainingQuantity)
                throw new InvalidOperationException($"Cannot receive {receiveItem.Quantity} — only {line.RemainingQuantity} remaining");

            line.ReceivedQuantity += receiveItem.Quantity;

            var rec = new ReceivingRecord
            {
                PurchaseOrderLineId = line.Id,
                QuantityReceived = receiveItem.Quantity,
                StorageLocationId = receiveItem.StorageLocationId,
                Notes = receiveItem.Notes,
                ReceiptNumber = receiptNumber,
                ActualFreight = actualFreight,
                FreightAllocationMethod = request.FreightAllocationMethod,
                // Filled in below once totals are known.
                AllocatedFreight = null,
            };
            newRecords.Add((receiveItem, line, rec));

            totalExtended += receiveItem.Quantity * line.UnitPrice;
            totalQty += receiveItem.Quantity;
        }

        // Allocation. Skip when no freight to allocate.
        if (actualFreight.HasValue && actualFreight.Value > 0m && newRecords.Count > 0)
        {
            switch (request.FreightAllocationMethod)
            {
                case FreightAllocationMethod.Manual:
                    foreach (var (req, _, rec) in newRecords)
                        rec.AllocatedFreight = req.ManualFreight ?? 0m;
                    break;

                case FreightAllocationMethod.ByQuantity:
                    if (totalQty > 0m)
                    {
                        foreach (var (_, _, rec) in newRecords)
                            rec.AllocatedFreight = Math.Round(actualFreight.Value * (rec.QuantityReceived / totalQty), 4);
                    }
                    break;

                case FreightAllocationMethod.ByWeight:
                    // Per-part weight not in this PR — fall back to
                    // ByExtendedValue. PR4 will populate Part.WeightLb /
                    // Part.WeightUom and re-enable the genuine weighted split.
                    goto case FreightAllocationMethod.ByExtendedValue;

                case FreightAllocationMethod.ByExtendedValue:
                default:
                    if (totalExtended > 0m)
                    {
                        foreach (var (_, line, rec) in newRecords)
                        {
                            var extended = rec.QuantityReceived * line.UnitPrice;
                            rec.AllocatedFreight = Math.Round(actualFreight.Value * (extended / totalExtended), 4);
                        }
                    }
                    break;
            }
        }

        foreach (var (_, _, rec) in newRecords)
            await repo.AddReceivingRecordAsync(rec, cancellationToken);

        var allReceived = po.Lines.All(l => l.RemainingQuantity <= 0);
        var anyReceived = po.Lines.Any(l => l.ReceivedQuantity > 0);

        if (allReceived)
        {
            po.Status = PurchaseOrderStatus.Received;
            po.ReceivedDate = clock.UtcNow;
        }
        else if (anyReceived)
        {
            po.Status = PurchaseOrderStatus.PartiallyReceived;
        }

        await repo.SaveChangesAsync(cancellationToken);

        var userId = int.Parse(httpContext.HttpContext!.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await mediator.Publish(new PurchaseOrderReceivedEvent(request.PurchaseOrderId, 0, userId), cancellationToken);
    }
}
